import os
import numpy as np
import open3d as o3d


bunny = o3d.data.BunnyMesh()
mesh = o3d.io.read_triangle_mesh(bunny.path)
mesh.compute_vertex_normals()

pcl = mesh.sample_points_poisson_disk(number_of_points=2000)

hull, _ = pcl.compute_convex_hull()
hull_ls = o3d.geometry.LineSet.create_from_triangle_mesh(hull)
hull_ls.paint_uniform_color((1, 0, 0))
o3d.visualization.draw_geometries([pcl, hull_ls])

obb = hull.get_oriented_bounding_box()
obb.color = (1,0,0)
o3d.visualization.draw_geometries([pcl, hull, hull.get_oriented_bounding_box()])
print(hull.get_oriented_bounding_box())
