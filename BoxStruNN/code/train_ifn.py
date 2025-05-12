import os
import glob
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from torchvision import models, transforms
from PIL import Image
import numpy as np

def makeSavedDirs(path):
    if not os.path.exists(path):
        os.makedirs(path)

class IFNModule(Dataset):
    def __init__(self, data_dir, transform=None):
        self.data_dir = data_dir
        self.transform = transform
        self.input_paths = sorted(glob.glob(os.path.join(data_dir, '*_input.png')))
        self.mask_paths = [p.replace('_input.png', '_mask.png') for p in self.input_paths]
        self.token_paths = [p.replace('_input.png', '.token') for p in self.input_paths]

    def __len__(self):
        return len(self.input_paths)

    def __getitem__(self, idx):
        input_img = Image.open(self.input_paths[idx]).convert('L')  # 1-channel
        mask_img = Image.open(self.mask_paths[idx]).convert('RGB')  # 3-channel

        if self.transform:
            input_tensor = self.transform(input_img)  # shape [1,H,W]
            mask_tensor = self.transform(mask_img)  # shape [3,H,W]
        else:
            raise ValueError("Transform must be provided.")

        combined_tensor = torch.cat([input_tensor, mask_tensor], dim=0)  # shape [4,H,W]

        token = np.fromfile(self.token_paths[idx], dtype=np.float32)
        token = torch.from_numpy(token)
        assert token.shape[0] == 256, f"Expected 256-dim token, got {token.shape[0]}"
        return combined_tensor, token

class ResNet50Latent(nn.Module):
    def __init__(self, pretrained=True, output_dim=256):
        super().__init__()
        self.backbone = models.resnet50(pretrained=pretrained)
        self.backbone.conv1 = nn.Conv2d(4, 64, kernel_size=7, stride=2, padding=3, bias=False)  # 4 channels input
        in_feats = self.backbone.fc.in_features
        self.backbone.fc = nn.Linear(in_feats, output_dim)

    def forward(self, x):
        return self.backbone(x)

def train_mode(args):
    transform = transforms.Compose([
        transforms.Resize((512, 512)),
        transforms.ToTensor(),
    ])
    dataset = IFNModule(args.data_dir, transform)
    loader = DataLoader(dataset, batch_size=args.batch_size, shuffle=True, num_workers=4)

    model = ResNet50Latent(pretrained=not args.from_scratch).to(args.device)
    criterion = nn.MSELoss()
    optimizer = optim.Adam(model.parameters(), lr=args.lr, weight_decay=args.weight_decay)

    for epoch in range(1, args.epochs + 1):
        model.train()
        total_loss = 0.0
        for i, (imgs, tokens) in enumerate(loader, 1):
            imgs, tokens = imgs.to(args.device), tokens.to(args.device)
            optimizer.zero_grad()
            out = model(imgs)
            loss = criterion(out, tokens)
            loss.backward()
            optimizer.step()
            total_loss += loss.item()
            if i % args.log_interval == 0:
                print(f"Epoch {epoch}[{i}/{len(loader)}] Loss: {total_loss / i:.4f}")
        ckpt = os.path.join(args.ckpt_dir, f"resnet50_epoch{epoch}.pth")
        makeSavedDirs(args.ckpt_dir)
        torch.save(model.state_dict(), ckpt)
    print("Training completed.")

def reference_mode(args):
    device = args.device
    model = ResNet50Latent(pretrained=False).to(device)
    model.load_state_dict(torch.load(args.ckpt, map_location=device))
    model.eval()

    transform = transforms.Compose([
        transforms.Resize((512, 512)),
        transforms.ToTensor()
    ])

    input_img = Image.open(args.input_image_path).convert('L')
    mask_img = Image.open(args.mask_image_path).convert('RGB')
    inp = torch.cat([
        transform(input_img),
        transform(mask_img)
    ], dim=0).unsqueeze(0).to(device)

    with torch.no_grad():
        token = model(inp).cpu().numpy().reshape(-1)

    makeSavedDirs(args.output_dir)
    base = os.path.splitext(os.path.basename(args.input_image_path))[0].replace('_input', '')
    out_path = os.path.join(args.output_dir, base + '.token')
    token.astype(np.float32).tofile(out_path)
    print(f"Saved token to {out_path}")

def parse_args():
    import argparse
    parser = argparse.ArgumentParser(description='ResNet50 IFN')
    sub = parser.add_subparsers(dest='mode', required=True)

    # train
    p_train = sub.add_parser('train')
    p_train.add_argument('--data_dir', type=str, required=True)
    p_train.add_argument('--batch_size', type=int, default=16)
    p_train.add_argument('--epochs', type=int, default=10)
    p_train.add_argument('--lr', type=float, default=1e-4)
    p_train.add_argument('--weight_decay', type=float, default=1e-5)
    p_train.add_argument('--log_interval', type=int, default=10)
    p_train.add_argument('--ckpt_dir', type=str, default='./checkpoints')
    p_train.add_argument('--from_scratch', action='store_true')
    p_train.add_argument('--device', type=str, default='cuda' if torch.cuda.is_available() else 'cpu')

    # reference
    p_ref = sub.add_parser('reference')
    p_ref.add_argument('--input_image_path', type=str, required=True)
    p_ref.add_argument('--mask_image_path', type=str, required=True)
    p_ref.add_argument('--ckpt', type=str, required=True)
    p_ref.add_argument('--output_dir', type=str, default='./output_tokens')
    p_ref.add_argument('--device', type=str, default='cuda' if torch.cuda.is_available() else 'cpu')

    return parser.parse_args()

if __name__ == '__main__':
    args = parse_args()
    if args.mode == 'train':
        train_mode(args)
    elif args.mode == 'reference':
        reference_mode(args)
