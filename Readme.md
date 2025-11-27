# PhotoResizer4K

A powerful command-line tool that intelligently converts photos to 4K UHD (3840x2160) resolution with automatic 16:9 aspect ratio cropping and content-aware positioning.

## Features

- **Smart Content-Aware Cropping**: Automatically detects and preserves the most interesting parts of your images when cropping to 16:9 aspect ratio
- **4K UHD Output**: Resizes all images to standard 4K resolution (3840x2160)
- **High-Quality Processing**: Uses Lanczos filtering for superior resizing quality
- **Automatic Sharpening**: Applies unsharp mask to maintain crisp details
- **EXIF Preservation**: Maintains important metadata from original images
- **Wide Format Support**: Handles JPEG, PNG, HEIC/HEIF, BMP, TIFF, and RAW formats (CR2, NEF, ARW)
- **Auto-Orientation**: Automatically applies correct EXIF orientation
- **Progressive JPEG**: Outputs optimized progressive JPEGs for faster web loading
- **Batch Processing**: Process entire folders of images in one command

## Requirements

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Supported platforms: Windows, macOS, Linux

## Installation

1. Clone the repository:
```bash
git clone https://github.com/yourusername/PhotoResizer4K.git
cd PhotoResizer4K
```

2. Build the project:
```bash
dotnet build
```

## Usage

### Basic Usage

```bash
dotnet run <input_folder> <output_folder>
```

### Examples

```bash
# Using absolute paths
dotnet run /Users/username/Photos/Input /Users/username/Photos/Output

# Using relative paths
dotnet run ./input ./output

# Using home directory shorthand
dotnet run ~/Pictures/Vacation ~/Pictures/Vacation_4K
```

### Supported Input Formats

- **JPEG**: .jpg, .jpeg
- **PNG**: .png
- **HEIC/HEIF**: .heic, .heif (Apple formats)
- **BMP**: .bmp
- **TIFF**: .tiff
- **RAW Formats**: .cr2 (Canon), .nef (Nikon), .arw (Sony)

### Output Format

All images are converted to high-quality JPEG files with:
- Resolution: 3840x2160 (4K UHD)
- Aspect Ratio: 16:9
- Quality: 92/100
- Format: Progressive JPEG
- Naming: `original_filename_4K.jpg`

## How It Works

1. **Auto-Orientation**: Reads and applies EXIF orientation data
2. **Aspect Ratio Analysis**: Compares source image ratio to target 16:9 ratio
3. **Smart Cropping**:
   - For wider images: Analyzes horizontal regions to find areas with highest visual interest
   - For taller images: Crops from upper portion to preserve main subjects
4. **High-Quality Resize**: Uses Lanczos filter for superior quality
5. **Sharpening**: Applies subtle unsharp mask (1.5 radius, 0.5 threshold)
6. **Optimization**: Saves as progressive JPEG with 92% quality

## Content-Aware Cropping Algorithm

The tool uses a sophisticated approach to determine the best crop position:

- Converts image to grayscale
- Analyzes multiple crop positions across the image
- Calculates visual interest using statistical variance
- Selects the region with highest detail and contrast
- Ensures important content is preserved in the final crop

## Building for Production

### Create a self-contained executable:

```bash
# For macOS (ARM)
dotnet publish -c Release -r osx-arm64 --self-contained

# For macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained

# For Windows
dotnet publish -c Release -r win-x64 --self-contained

# For Linux
dotnet publish -c Release -r linux-x64 --self-contained
```

The executable will be in `bin/Release/net9.0/<runtime>/publish/`

## Example Output

Before processing:
- Image1.jpg (4000x3000, 4:3 ratio)
- Image2.heic (3024x4032, 3:4 ratio)
- Image3.cr2 (6000x4000, 3:2 ratio)

After processing:
- Image1_4K.jpg (3840x2160, 16:9 ratio)
- Image2_4K.jpg (3840x2160, 16:9 ratio)
- Image3_4K.jpg (3840x2160, 16:9 ratio)

## Dependencies

- [Magick.NET-Q16-AnyCPU](https://github.com/dlemstra/Magick.NET) (v14.9.1): .NET wrapper for ImageMagick

## License

This project is open source. Please check the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Troubleshooting

### Error: "Source folder does not exist"
- Verify the input folder path is correct
- Use quotes around paths with spaces: `dotnet run "~/My Photos" ~/Output`

### Images appear over-sharpened
- Adjust the UnsharpMask parameters in [Program.cs:111](Program.cs#L111)
- Reduce the first parameter (radius) or second parameter (amount)

### Quality issues
- Modify the Quality setting in [Program.cs:114](Program.cs#L114)
- Range: 1-100 (current: 92)

### Out of memory errors
- Process smaller batches of images
- Close other memory-intensive applications
- Consider increasing available system RAM
