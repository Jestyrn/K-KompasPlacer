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
  <img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/S-NotReady.png?raw=true" width="350">
  <img src="https://github.com/Jestyrn/K-KompasPlacer/blob/master/Readme/D-NotReady.png?raw=true" width="350">
</p>
<h2>✅Ready to use v1.0 WPF✅</h2>
<h2>How it work</h2>

- The user uploads a `.dxf` file with details (yes, with the very drawing that someone lovingly made in CAD for three nights in a row).
- Specifies the size of the frame — it's like telling the program: "these are the boundaries of reality, work inside."
- Next, the netDxf library comes into play (thanks to the authors, you are the best):
  - We read all the elements from the drawing;
  - We take out the components (sometimes even what we have long forgotten to remove);
  - We read the specifications (if there are any, we don't insist, but it's nice).

- Based on the entered dimensions, we create a frame — not a spiritual one, but a rectangular one, in millimeters.
- The automatic Tetris algorithm is activated:
  - We calculate how much free space there is;
  - We are looking for a suitable part (which will not go over the edges and will not climb on the neighbor);
  - We post it (yes, with all love and respect for accuracy);
  - We're counting the available space again;
  - We repeat while there is someone to put down.

- If there are no suitable parts left:
  - Create a new frame (just like that, without downloads and microtransactions);
  - We continue the placement as if nothing had happened.

- When the details run out:
  - The program says, "That's it, we're done, live in peace";
  - And saves the result in `.dxf` so that you can open it later, print it out, or proudly show it to your superiors.
<hr>

<p align="center">
  <strong>Jestyrn – 2025</strong><br>
  <sub>License - MIT</sub>
</p>
