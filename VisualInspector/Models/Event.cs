﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using AForge.Video.FFMPEG;
using NLog;

namespace VisualInspector.Models
{
    public enum WarningLevels
    {
        Normal, Middle, High
    }

    public class Event
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

        public static readonly int FramesForPreview = 5;


        #region Properties

        public WarningLevels WarningLevel { get; set; }
        public int Lock { get; set; }
        public int Sensor { get; set; }
        public int Room { get; set; }
        public DateTime DateTime { get; set; }
        public int AccessLevel { get; set; }
        public string VideoFileName { get; set; }


        public void InitFramesList(List<BitmapImage> framesList)
        {
            framesList.Clear();
			if (File.Exists(VideoFileName))
			{
				var videoReader = new VideoFileReader();
				videoReader.Open(VideoFileName);
				var framesCount = videoReader.FrameCount;
				framesCount = 250;
				var multiplicity = (int)(framesCount / FramesForPreview);
				for (int i = 0; i < framesCount; i++)
				{
					var nextFrame = videoReader.ReadVideoFrame();
					if (i % multiplicity == 0)
					{
						using (var memory = new MemoryStream())
						{
							nextFrame.Save(memory, ImageFormat.Png);
							memory.Position = 0;
							var bitmapImage = new BitmapImage();
							bitmapImage.BeginInit();
							bitmapImage.StreamSource = memory;
							bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
							bitmapImage.EndInit();
                            bitmapImage.Freeze();
                          	framesList.Add(bitmapImage);
						}
					}
					nextFrame.Dispose();
				}
			}

        }

        #endregion

    }
}
