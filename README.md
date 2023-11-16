# TTI2IMG
Converts .tti teletext files to image

## Installation

Download the latest release from the [Releases](https://github.com/morski/TTI2IMG/releases/tag/1.0.0) page.

## Usage

./TTI2IMG [options]

Options:

`--help` Show help message

`--version` Show version information

`-i, --input` **Required.** Path to tti file or directory containing tti files

`-o, --output` **Required.** Path to where image(s) should be saved

`-f, --format` What format the image(s) should be saved as. Valid options are PNG, JPG, GIF, BMP, PBM, TGA, TIFF, WEBP. Default is PNG

`-h, --height` The height of the image. Default is 768px

`-w, --width` The width of the image. Default is 576px

Images look best when using a 4:3 aspect ratio


## Examples
`./TTI2IMG -i P100.tti -o teletext-images -f JPG -h 1024 -w 768`

There are test files to use in the [TTI test files](https://github.com/morski/TTI2IMG/tree/master/TTI2IMG/TTI%20test%20files) folder
