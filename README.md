# BoxPlantReconstruction

Codebase of our paper in CVPR 2025: *"Neural Hierarchical Decomposition for Single Image Plant Modeling. Zhihao Liu et al."*

If you are interested in **3D plant reconstruction from real-world data**, please also see my related GitHub repositories: 

- [[Point-cloud Tree Reconstruction]](https://github.com/RyuZhihao123/Point-cloud-3D-tree-reconstruction): a free software for reconstructing plants from point cloud scans.
- [[SVDTree]](https://github.com/RyuZhihao123/SVDTree): a new technique for reconstructing plants using diffusion priors.



## Update logs:

Done:

- ✅ 2025/03/09. Created the repository.
- ✅ 2025/04/01. Released the **[[Source code]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling)** for Plant Reconstruction completely.

To do:
- ⏳Online Demo, which will be a WebGL application.
- ⏳(Optional) Improve the code readability.

<p align="center">
  <img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/1.png" width="550" style="display:block; margin:auto;">
</p>


## Hierarchical Boxes-based Plant Reconstruction

## ☐  Overview
Our project is a successful application of combining the hierarchical learning with procedural modeling for producing practically-usable, realistic 3D plant models.
Therefore, our project contains two key steps:

- **Part 1: BoxGen Networks:** The networks are used to produce the hierarchical boxes of plant models.
  
- **Part 2: Shape-driven Plant Modeling:** This part is to produce the final 3D plant geometries from the box structures, which is the most difficult step in terms of programming. **The algorithm of this part is entirely designed and implemented by myself.**

## ☐  Our Dataset Generation Tool.

**Download Link:** We have developed a series of powerful 3D plant generators in advance, to automatically synthesize the training dataset of diverse plant species.
The latest version is available at my another repository [[Modular-Tree-Modeler]](https://github.com/RyuZhihao123/Modular-Tree-Modeler-25).

Using my dataset generation software, you can directly export your own 3D plant datasets into local disk for any personal use. The following figure shows an example of training pair of segmentation masks and 3D geometries.

<p align="center">
  <img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/4.png" width="650" style="display:block; margin:auto;">
</p>

But please note that, we hold the copyright of this dataset tool. **Thus, if you intend to use it in your presearch roject, please email us for permission.**


## ☐  Part-1: BoxGen Networks.

Our project includes multiple network modules for different inference steps.

#### Segmentation

The segmentation network is simply the [Swin-Transformer](https://github.com/microsoft/Swin-Transformer). 

#### Hierarchical Box Decoder
We implemented the hierarchical network heavily based on [StructureNet](https://arxiv.org/abs/1908.00575), aftering modifying the backbone as [MolGAN](https://arxiv.org/abs/1805.11973).
Thus, please also refer to their projects as well to get more comprehensive instructions and reference code.

IFN module:
```
# training
python your_script.py train --data_dir data/ --epochs 20 --batch_size 32

# inference (get latent vector from original image)
python your_script.py reference --image_path data/img001.png --ckpt checkpoints/resnet50_epoch10.pth --output_dir tokens/
```

DEC module:
```
# training
python your_script.py train --data_dir data/ --epochs 20 --batch_size 32

# inference (get latent vector from original image)
python your_script.py reference --image_path data/img001.png --ckpt checkpoints/resnet50_epoch10.pth --output_dir tokens/
```

## ☐ Part-2: Shape-guided 3D Plant Construction 🔥

<p align="center">
  <img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/5.png" width="650" style="display:block; margin:auto;">
</p>

Please install IDEs (include **Unity** 2022.3.10f1+ and **PyCharm**).
then you can directly open the [[code folder]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling) to easily run the program.

#### Shape-guided Procedural 3D modeling.

#### Code Hierarchy

```
RootPath: BoxPlantModeling
├── BoxPlantModeling.sln  (You can open this .sln file in Visual Studio to view the entire codes more clearly.)
├── Assets (THIS FOLDER IS THE KEY PART!)
    ├── HDRPDefaultResources (HDRP releted settings)
    ├── Scripts (The code folder)
        ├── CameraCtrl.cs (A simple camera controller)
        ├── Main.cs (Entrance)
        ├── Plant.cs (The data structure for controlling the plant geometries)
        ├── UI (The UI widget events)
        ├── Lib (The supportive math code, e.g., KDTree, Hermit interpolation, Heap, etc.)
        ├── Utils (The geometry synthesizer.)
    ├── Materials / Resources / Shaders (These three folders contain the ShaderGraph and materials for rendering the plants.)
├── Packages 
├── ProjectSettings & UserSettings (Here you can change your project setting.)
```

- For this step, please pay attention to this [script](https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/BoxPlantModeling/Assets/Scripts/Plant.cs), which is the main entrance of the entire program.

- You can call the following scripts in **main.cs** to construct the final 3D plant model. And here is an [example box](https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Test/test.box) structure that you can use for a quick test.


```C++
Plant plant = new Plant();   // Create a plant proxy.
UTIL.LoadPlant(plant, "PATH_TO_BOX/test.box");  // load the box structure.

this.ConstructPlantGameObjects();  // construct the plant geometry associated with a Unity GameObject.
```

After running the script above, you will see the results in GUI as follows:

<img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/3.png" width="900" style="display:block; margin:auto;">

<img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/2.png" width="900" style="display:block; margin:auto;">


### Citation

```
@inproceedings{liu2025neural,
  title={Neural Hierarchical Decomposition for Single Image Plant Modeling},
  author={Liu, Zhihao and Cheng, Zhanglin and Yokoya, Naoto},
  booktitle={Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition},
  year={2025}
}
```




