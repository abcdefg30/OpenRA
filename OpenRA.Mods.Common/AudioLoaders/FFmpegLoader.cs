#region Copyright & License Information
/*
 * Copyright 2007-2020 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Diagnostics;
using System.IO;
using ff

namespace OpenRA.Mods.Common.AudioLoaders
{
	public class FFmpegLoader : ISoundLoader
	{
		bool ISoundLoader.TryParseSound(Stream stream, out ISoundFormat sound, string filename)
		{
			try
			{
				var mediaInfo = FFProbe.Analyse(inputFile);
				/*var ffmpeg = new Process()
				{
					StartInfo =
					{
						FileName = "ffmpeg.exe",
						CreateNoWindow = true,
						UseShellExecute = false,
						RedirectStandardInput = true,
						RedirectStandardOutput = true,
						Arguments = "-i {0}".F(filename)
					}
				};

				ffmpeg.Start();

				ffmpeg.WaitForExit();*/
			}
			catch
			{
				// Not supported by FFmpeg
			}

			sound = null;
			return false;
		}
	}

	public class FFmpegFileStream : ISoundFormat
	{
		int ISoundFormat.Channels { get { return channels; } }
		int ISoundFormat.SampleBits { get { return bits; } }
		int ISoundFormat.SampleRate { get { return rate; } }
		float ISoundFormat.LengthInSeconds { get { return length; } }

		int channels;
		int bits;
		int rate;
		float length;

		void IDisposable.Dispose()
		{
			throw new NotImplementedException();
		}

		Stream ISoundFormat.GetPCMInputStream()
		{
			throw new NotImplementedException();
		}
	}
}
