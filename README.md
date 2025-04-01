# BoxPlantReconstruction [CVPR 2025]
Implementation for our paper in CVPR 2025: *"Neural Hierarchical Decomposition for Single Image Plant Modeling. Zhihao Liu et al."*

If you are interested in 3D plant reconstruction, please also see my related GitHub repositories: 
- [[Point-cloud Tree Reconstruction]](https://github.com/RyuZhihao123/Point-cloud-3D-tree-reconstruction): a free software for reconstructing plants from point cloud scans.
- [[SVDTree]](https://github.com/RyuZhihao123/SVDTree): a new technique for reconstructing plants using diffusion priors.




## Update logs:

Done:

- ✅ 2025/03/09. Created the repository.
- ✅ 2025/04/01. Released the **[[Source code]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling)** for Plant Reconstruction completely.

To do:
- ⏳Online Demo, which will be a WebGL application.
- ⏳Write a detailed instruction.
- ⏳(Optional) Improve the code readability.


I will get back to handle the rest ToDo works after the **ACM UIST submission deadline** (Apr. 09).

<img src="https://github.com/RyuZhihao123/Plant-Recon-25/blob/main/Figures/1.png" width="500" style="display:block; margin:auto;">


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



### Usage

Please install IDEs (include **Unity** 2022.3.10f1+ and **PyCharm**).
then you can directly open the [[code folder]](https://github.com/RyuZhihao123/Plant-Recon-25/tree/main/BoxPlantModeling) to execute the program.


🔴 Note: The user guidance will be completed after my recent paper submission. Before that, you can temporarily refer to my raw source code.

will introduce how to customize your data,  get boxes and geometries.










