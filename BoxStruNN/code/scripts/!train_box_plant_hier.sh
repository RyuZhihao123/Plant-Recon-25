
python ./train_box.py \
  --exp_name 'box_vae_storagefurniture_hier' \
  --category 'Storagefurniture' \
  --data_path '../data/partnetdata/storagefurniture_hier' \
  --train_dataset 'train_no_other_less_than_10_parts.txt' \
  --val_dataset 'val_no_other_less_than_10_parts.txt' \
  --epochs 10 \
  --model_version 'model_box'

# python ./train_box.py  --exp_name box_vae_chair  --category Chair  --data_path ../data/partnetdata/chair_hier   --train_dataset train_no_other_less_than_10_parts.txt  