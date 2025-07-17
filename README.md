<p align="center">
<img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/my-logo.png" height="100">
<img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/KWORK.png" height="100">
</p>

<h1 align="center">Jestyrn for - Kwork</h1>

<h2 align="center">Preview</h2>

<p align="center">
  <img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/S-NotReady.png?raw=true" width="300">
  <img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/D-NotReady.png?raw=true" width="300">
</p>

<h2>How it work</h2>

- The user loads a `.dxf` file with details (yes, the same drawing that someone lovingly made in CAD for three nights in a row).
- They specify the frame dimensions, which is like telling the program, "Here are the boundaries of reality, work within them."
- Then, the `netDxf` library comes into play (thanks to its authors, who are the best):
 - We read all the elements from the drawing;
 - We extract the components (sometimes even those that have been forgotten to delete);
 - Read the specifications (if there are any — we don't insist, but it's nice).

- Based on the entered dimensions, create a frame — not spiritual, but rectangular, in millimeters.
- The automatic Tetris algorithm is enabled:
 - Count how much free space there is;
 - Look for a suitable part (which will not go beyond the edges or climb on its neighbor);
 - Place it (yes, with all the love and respect for precision);
 - Count the free space again;
 - Repeat as long as there is someone to put.

- If there are no suitable parts left:
 - Spawn a new frame (that's it, no downloads or microtransactions

<hr>

<p align="center">
  <strong>Jestyrn – 2025</strong><br>
  <sub>License - MIT</sub>
</p>
