/**
 *
 * \brief This is a private library for functions that I have needed to used in
 *        some parts of code: conversions, maths methods...
 *
 *        This library includes third-party code.
 *
 * \note This library can be extended in the future.
 *
 * \author $Author: Mario Gonzalez $
 *
 * \version $Revision: 0.5 $
 *
 * \date $Date: 2016/12/14 $
 *
 * Contact: mario.gonzalez.jim@gmail.com
 *
 */

#ifndef VARIOS_H
#define VARIOS_H
#include "libraries.h"


///< It is used on base64 decode
static const std::string base64_chars =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    "abcdefghijklmnopqrstuvwxyz"
    "0123456789+/";

/** \brief This function asks if character belongs to base64 alphabet
 *
 * \param c char character
 * \return true or false
 *
 * \author $Author: René Nyffenegger $
 *
 * reference:  http://www.adp-gmbh.ch/cpp/common/base64.html
 */
bool is_base64(unsigned char c);

/** \brief This function decodes string using base64
 *
 * \param encoded_string string previously encoded to base64
 * \return string decoded
 *
 * \author $Author: René Nyffenegger $
 *
 * reference:  http://www.adp-gmbh.ch/cpp/common/base64.html
 */
std::string base64_decode(std::string const& encoded_string);

/** \brief This function returns the number of lines a file has.
 *
 * \param file pointer to file we want to check
 * \return numbers of lines
 *
 */
int contarNumLineas(FILE* file);

/** \brief This function combines a (BGR) background image with a (BGRA)
 *         semi-transparent foreground image.
 *
 * \param background (BGR) background image
 * \param foreground (BGRA) semi-transparent foreground image
 * \param output image destination
 * \param location coordinates where foreground image will be located
 *
 * \author $Author: Michael Jepson $
 *
 * reference:  http://jepsonsblog.blogspot.com.es/2012/10/overlay-transparent-image-in-opencv.html
 */
void overlayImage(const cv::Mat &background, const cv::Mat &foreground,
                  cv::Mat &output, cv::Point2i location);

/** \brief Rotate OpenCV Mat by 90, 180 or 270 degrees.
 *
 *
 * \param matImage OpenCV Mat
 * \param rotflag (BGRA) 1 for 90 degrees, 2 for 180 and 3 for 270
 *
 * \author $Author: TimZaman $
 *
 * reference:  http://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
 */
void rot90Image(cv::Mat &matImage, int rotflag);

/** \brief Rotate OpenCV Mat by an arbitrary degree
 *
 *
 * \param src OpenCV Mat source
 * \param angle arbitrary degree, from 1 to 360
 * \param dst OpenCV Mat dst
 *
 * \author $Author: TimZaman $
 *
 * reference:  http://stackoverflow.com/questions/15043152/rotate-opencv-matrix-by-90-180-270-degrees
 */
void rotateImage(cv::Mat& src, double angle, cv::Mat& dst);

#endif
