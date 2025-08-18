<p align="center">
<img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/my-logo.png" height="150">
<img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/KWORK.png" width="150">
</p>

<h1 align="center">Jestyrn for - Kwork</h1>

<p align="center">
  English | <a href="/README-RU.md">Russian</a>
</p>

<h2>Preview</h2>

<p align="center">
  <img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/ProgramView.png" width="700">
</p>

<h2>✅ Ready for Testing ✅</h2>

<h2>How to Use It?</h2>

1. Specify the path to the `.dxf` file  
2. Specify the folder where the result should be saved  
3. Wait until the data is displayed on the screen  
4. Set the desired parameters in the right panel  
5. Click the `Start Processing` button  
6. Wait for completion, then check the “output” folder  
   _(a file named_ `ReadyToUpload.dxf` _will appear)_

<h2>How It Works</h2>

1. **File Reading**  
   - Calculate the minimum and maximum coordinates  
   - Build a `BoundingBox` based on these coordinates (a simplified version of the part as a rectangle)  
   - Define additional helper arguments  

2. **Initial Processing**  
   - Remove unnecessary data (any part with fewer than 6 elements is discarded — a point counts as an element)  
   - Sort parts by size  

3. **Calculation Process**  
   - Pass the provided parameters to the corresponding settings (margins to margins, sizes to sizes)  
   - Create the first `Frame`  
   - Place the first part in the top-left corner (all current and future calculations are based on the `BoundingBox` coordinates)  
   - Divide the remaining space in the frame into horizontal and vertical areas (excluding the occupied cell)  
   - Repeat the process until the first frame is filled (then create a new one and continue until all parts are placed)  

4. **Output**  
   - After processing all parts, store them in a `FramePackage` (Frame + Parts inside)  
   - Add each `FramePackage` to `netDXF`  
   - Save the result to `ReadyToUpload.dxf`

5. **Examples of works**
   - You can view examples of works in the `assets` folder
   - The `Example Details.dxf` file is an example of details (the number of details has been reduced to maintain privacy)
   - The `Example Work.dxf` file is an example of the program's functionality

<hr>

<p align="center">
  <strong>Jestyrn – 2025</strong><br>
  <sub>License - MIT.</sub>
</p>
