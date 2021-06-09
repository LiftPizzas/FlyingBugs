/*
//check to see if the normal is facing us:
Vector3 u = (p[vb] - p[va]);
Vector3 w = (p[vc] - p[va]);
Vector3 Normal;
Normal.X = (u.Y* w.Z) - (u.Z* w.Y);
											Normal.Y = (u.Z* w.X) - (u.X* w.Z);
											Normal.Z = (u.X* w.Y) - (u.Y* w.X);

											if (Normal.Z > 0)
*/