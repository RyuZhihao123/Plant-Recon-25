# BoxPlantReconstruction [CVPR 2025]
Codebase of our paper in CVPR 2025: *"Neural Hierarchical Decomposition for Single Image Plant Modeling. Zhihao Liu et al."*

If you are interested in 3D plant reconstruction, please also see my related GitHub repositories: 
- [[Point-cloud Tree Reconstruction]](https://github.com/RyuZhihao123/Point-cloud-3D-tree-reconstruction): a free software for reconstructing plants from point cloud scans.
- [[SVDTree]](https://github.com/RyuZhihao123/SVDTree): a new technique for reconstructing plants using diffusion priors.




## Update logs:

Done:

- ✅ 2025/03/09. Created the repository.
- ✅ 2025/04/01. Released the **[[Source code]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling)** for Plant Reconstruction completely.

To do:
- ⏳Online Demo, which will be a WebGL application.
- ⏳(Optional) Improve the code readability.



<img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/1.png" width="550" style="display:block; margin:auto;">


## Hierarchical Boxes-based Plant Reconstruction:

### Code Hierarchy

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

### Dataset Preparation
- Please refer to [my another repository](https://github.com/RyuZhihao123/Modular-Tree-Modeler-25) to automatically generate the plant dataset, which can jointly produce 3D plant models, segmentations and voxels.



### Usage

- Please install IDEs (include **Unity** 2022.3.10f1+ and **PyCharm**).
then you can directly open the [[code folder]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling) to easily run the program.


- Please refer to this [latex](https://github.com/RyuZhihao123/Modular-Tree-Modeler-25) document for more explanations about the usage.

- The neural network is heavily based on [StructureNet](https://arxiv.org/abs/1908.00575) and [MolGAN](https://arxiv.org/abs/1805.11973). The procedural 3D plant modeling algorithm is implemented by myself and please refer to this [script](https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/BoxPlantModeling/Assets/Scripts/Plant.cs).

Here is an [example box](https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Test/test.box) structure inferred by network. You can call the following scripts in **main.cs** to construct the final 3D plant model:

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
@inproceedings{li2025neural,
  title={Neural Hierarchical Decomposition for Single Image Plant Modeling},
  author={Liu, Zhihao and Cheng, Zhanglin and Yokoya, Naoto},
  booktitle={Proceedings of the IEEE/CVF Conference on Computer Vision and Pattern Recognition},
  year={2025}
}
```




