/**
 * \file    Encoder.cs
 *
 * \brief   Implements the encoder class.
 */

using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using TurboJpegWrapper;
using System.IO;



/**
 * \class   CustomEncoder
 *
 * \brief   This class is a custom image encoder using .Net framework, EmguCV library and libjpeg-turbo (one of the fastest JPEG image codec available). 
 *
 * \author  Mgj
 * \date    08/11/2016
 */

public class CustomEncoder  {

    enum ImageFormat
    {
        jpg = 0,
        png = 1,
        raw = 2,
    };

    int width=0, height=0, quality = 0;
    ImageFormat format=0;
    Encoder myEncoder;
    ImageCodecInfo jpgEncoder;
    ImageCodecInfo pngEncoder;
    EncoderParameters encoderParameters;
    EncoderParameter myEncoderParameter;

    /**
     * \fn  public CustomEncoder(int width_,int height_, int quality_, int format_)
     *
     * \brief   Constructor.
     *
     * \author  Mgj
     * \date    08/11/2016
     *
     * \param   width_      The width.
     * \param   height_     The height.
     * \param   quality_    The quality.
     * \param   format_     Describes the format to use.
     */

    public CustomEncoder(int width_,int height_, int quality_, int format_) {
   
        width = width_;
        height = height_;
        quality = quality_;
        if (format_ == 0)
        {
            format = ImageFormat.jpg;
        }
        else if (format_ == 1) {
            format = ImageFormat.png;
        }
        else {
            format = ImageFormat.raw;
        }

        jpgEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
        encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 50L);


    }

    /**
     * \fn  private ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
     *
     * \brief   Gets an encoder for a specific format.
     *
     *
     * \param   format  Image format.
     *
     * \return  The codec that it will be used for encoding.
     */

    private ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
    {

        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {

            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    /**
     * \fn  private byte[] encodeImage(Image<Rgb, Byte> img)
     *
     * \brief   It encodes an image to specific format
     *
     *
     * \param   img  Image we want to encode 
     * \param   imageFormat  image format we want get
     *
     * \return  byte array data
     */
    private byte[] encodeImage(Image<Rgb, Byte> img, ImageFormat imageFormat) {

        byte[] imageBytes=null;

        if (imageFormat == ImageFormat.jpg)
        {
            TJCompressor codec = new TJCompressor();
            imageBytes = codec.Compress(img.Bitmap, TJSubsamplingOptions.TJSAMP_411, quality, TJFlags.NONE);
        }
        else if (imageFormat == ImageFormat.png)
        {
            MemoryStream ms = new MemoryStream();
            img.Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            imageBytes = ms.GetBuffer();
        }
        else {
            //raw-> original, no encode
            return img.Bytes;

        }


        return imageBytes;
    }


    private string encodeDataToBase64(byte[] data) { 

        return Convert.ToBase64String(data);
    }

    /**
    * \fn   public string encodeFrame(Image<Rgb, Byte> img)
    *
    * \brief   It encodes an image to specific format which has been configured on constructor 
    *          and return image data as string using a base64 conversion.
    *
    * \param   img  Image we want to encode 
    *
    * \return  image data as string
    */
    public string encodeFrame(Image<Rgb, Byte> img) {
        byte[] data = this.encodeImage(img, format);
        return this.encodeDataToBase64(data);

    }

    /**
    * \fn   public void setQuality(int newquality)
    *
    * \brief   Set the quality of the image encoding, only valid for jpg format.
    *
    * \param   newquality  Value between 1 and 99 
    *
    */
    public void setQuality(int newquality) {
        quality = newquality;
    }

    /**
    * \fn   public void setQuality(int newquality)
    *
    * \brief   Set the image format.
    *
    * \param   newformat  0 = JPG, 1=PNG, other= RAW (original)
    *
    */
    public void setImageFormat(int newformat) {
        if (newformat == 0)
        {
            format = ImageFormat.jpg;
        }
        else if (newformat == 1)
        {
            format = ImageFormat.png;
        }
        else {
            format = ImageFormat.raw;
        }
    }

}
