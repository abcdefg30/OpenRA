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
using System.IO;
using FFMpegCore;
using FFMpegCore.Pipes;

namespace OpenRA.Mods.Common.AudioLoaders
{
	public class FFmpegLoader : ISoundLoader
	{
		bool ISoundLoader.TryParseSound(Stream stream, out ISoundFormat sound, string filename)
		{
			try
			{
				using (var audioStream = new MemoryStream())
				{
					var source = FFMpegArguments.FromPipe(new StreamPipeSource(stream)).ForceFormat("wav");
					var processor = source.OutputToPipe(new StreamPipeSink(audioStream));
					if (processor.ProcessSynchronously(false))
					{
						audioStream.Position = 0;
						var bytes = audioStream.ReadAllBytes();

						// Using RIFF:
						// Problem: dataSize is set to -1
						// dataSize is at locations 40-43
						// The length of the header is 44 bytes
						// For some reason this is using a different header structure in my test files, figure this out
						var size = bytes.Length - 78;
						bytes[74] = (byte)(size >> 24);
						bytes[75] = (byte)(size >> 16);
						bytes[76] = (byte)(size >> 8);
						bytes[77] = (byte)size;

						var x = new MemoryStream(bytes);
						var asd = new WavLoader();
						return ((ISoundLoader)asd).TryParseSound(x, out sound, filename);
					}
				}
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
		Stream ISoundFormat.GetPCMInputStream() { return audioStream; }

		readonly int channels;
		readonly int bits;
		readonly int rate;
		readonly float length;
		readonly Stream audioStream;

		public FFmpegFileStream(MediaAnalysis info, Stream audioStream, float lengthInSeconds)
		{
			channels = info.PrimaryAudioStream.Channels;
			bits = info.PrimaryAudioStream.BitRate;
			rate = info.PrimaryAudioStream.SampleRateHz;
			length = lengthInSeconds;

			audioStream.Position = 0;
			this.audioStream = audioStream;
		}

		void IDisposable.Dispose()
		{
			audioStream.Dispose();
		}
	}
}
