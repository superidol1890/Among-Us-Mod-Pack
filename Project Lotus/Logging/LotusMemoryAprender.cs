// using log4net.Appender;
// using log4net.Core;
// using System;
// using System.Collections.Generic;
// using VentLib.Logging.Appenders;
// using System.IO;

// public class LotusMemoryAprender : AppenderSkeleton
// {
//     private readonly List<LoggingEvent> _events = new List<LoggingEvent>();
//     private FileInfo? _logFile;
//     private string _fileNamePattern;

//     public LotusMemoryAprender()
//     {
//     }

//     public FileInfo? LogFile
//     {
//         get => _logFile;
//         set => _logFile = value;
//     }

//     public string FileNamePattern
//     {
//         get => _fileNamePattern;
//         set => _fileNamePattern = value;
//     }

//     protected override void Append(LoggingEvent loggingEvent)
//     {
//         lock (_events)
//         {
//             _events.Add(loggingEvent);
//         }
//     }

//     public int Flush(bool flushAll, int count)
//     {
//         lock (_events)
//         {
//             int flushCount = flushAll ? _events.Count : Math.Min(count, _events.Count);

//             string filePath = _logFile?.FullName ?? GetDefaultLogFilePath();

//             using (StreamWriter writer = new StreamWriter(filePath, append: true))
//             {
//                 for (int i = 0; i < flushCount; i++)
//                 {
//                     var loggingEvent = _events[i];
//                     writer.WriteLine(loggingEvent.RenderedMessage);
//                 }
//             }

//             _events.RemoveRange(0, flushCount);
//             return flushCount;
//         }
//     }

//     public void CreateNewFile()
//     {
//         if (!string.IsNullOrEmpty(_fileNamePattern))
//         {
//             _logFile = new FileInfo(string.Format(_fileNamePattern, DateTime.Now));
//         }
//         else
//         {
//             throw new InvalidOperationException("FileNamePattern must be set to create a new file.");
//         }
//     }

//     private string GetDefaultLogFilePath()
//     {
//         return "defaultLog.txt";
//     }

//     public static LotusMemoryAprender ConvertFrom(FlushingMemoryAppender oldAppender)
//     {
//         if (oldAppender == null) throw new ArgumentNullException(nameof(oldAppender));

//         var newAppender = new LotusMemoryAprender
//         {
//             LogFile = oldAppender.LogFile,
//             FileNamePattern = oldAppender.FileNamePattern
//         };

//         return newAppender;
//     }
// }
