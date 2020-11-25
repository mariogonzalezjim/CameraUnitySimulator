using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

public static class OpenCV
{
	/// <summary>
	/// Loads some image data from a memory mapped file
	/// </summary>
	/// <returns></returns>
	[DllImport("unity_plugin_with_opencv")]
	private static extern void decode(int fileSize, byte[] fileData, int imageWidth, byte[] imageStorage);

	/// <summary>
	/// Loads a image, by path, into a Texture2D.
	/// </summary>
	/// 
	/// Untiy already has a (better) version of this but I wanted to demonstrate something simple
	/// 
	/// <param name="texture2D"></param>
	/// <param name="filePath"></param>
	/// <param name="size"></param>
	public static Texture2D LoadFromFile(int size, string filePath)
	{
		var file = File.ReadAllBytes(filePath);
		var data = new byte[size * size * 3];

		decode(file.Length, file, size, data);

		Texture2D texture2D = new Texture2D(size, size, TextureFormat.RGB24, false);

		int read = 0;
		for (int i = 0; i < size; ++i)
			for (int j = 0; j < size; ++j)
			{
				var r = ((float)data[read++]) / 255.0f;
				var g = ((float)data[read++]) / 255.0f;
				var b = ((float)data[read++]) / 255.0f;

				texture2D.SetPixel(i, j, new Color(r, g, b));
			}

		texture2D.Apply();

		return texture2D;
	}
}