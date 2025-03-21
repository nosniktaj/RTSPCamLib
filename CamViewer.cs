using System.Drawing;
using System.Drawing.Imaging;

using FFmpeg.AutoGen;

namespace RTSPCamLib
{
   internal class CamViewer
   {
      internal EventHandler<Bitmap>? OnFrameReceived;

      #region Internal Methods
      unsafe internal void OpenRTSPStream(string rtspUrl)
      {
         var pFormatContext = ffmpeg.avformat_alloc_context();

         if (ffmpeg.avformat_open_input(&pFormatContext, rtspUrl, null, null) != 0)
         {
            throw new Exception("Couldn't open RTSP stream.");
         }
         if (ffmpeg.avformat_find_stream_info(pFormatContext, null) < 0)
         {
            throw new Exception("Couldn't find stream information.");
         }

         var videoStreamIndex = -1;
         for (var i = 0 ; i < pFormatContext->nb_streams ; i++)
         {
            if (pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
               videoStreamIndex = i;
               break;
            }
         }

         if (videoStreamIndex == -1)
         {
            throw new Exception("No video stream found.");
         }

         var codec = ffmpeg.avcodec_find_decoder(pFormatContext->streams[videoStreamIndex]->codecpar->codec_id);
         var codecContext = ffmpeg.avcodec_alloc_context3(codec);

         if (ffmpeg.avcodec_open2(codecContext, codec, null) < 0)
         {
            throw new Exception("Couldn't open codec.");
         }

         ReadFrames(pFormatContext, codecContext, videoStreamIndex);

         ffmpeg.avcodec_free_context(&codecContext);
         ffmpeg.avformat_close_input(&pFormatContext);
      }

      unsafe internal void ReadFrames(AVFormatContext* pFormatContext, AVCodecContext* codecContext, int videoStreamIndex)
      {
         var packet = ffmpeg.av_packet_alloc();
         var frame = ffmpeg.av_frame_alloc();
         SwsContext* swsContext = null;

         while (ffmpeg.av_read_frame(pFormatContext, packet) >= 0)
         {
            if (packet->stream_index == videoStreamIndex)
            {
               if (ffmpeg.avcodec_send_packet(codecContext, packet) == 0)
               {
                  while (ffmpeg.avcodec_receive_frame(codecContext, frame) == 0)
                  {
                     var width = frame->width;
                     var height = frame->height;
                     var img = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                     var bitmapData = img.LockBits(new Rectangle(0, 0, width, height),
                         ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                     if (swsContext == null)
                     {
                        swsContext = ffmpeg.sws_getContext(width, height, (AVPixelFormat) frame->format,
                                                           width, height, AVPixelFormat.AV_PIX_FMT_BGR24,
                                                           ffmpeg.SWS_BILINEAR, null, null, null);
                     }

                     var dstData = new byte_ptrArray4();
                     var dstLinesize = new int_array4();
                     dstData[0] = (byte*) bitmapData.Scan0;
                     dstLinesize[0] = width * 3;

                     _ = ffmpeg.sws_scale(swsContext, frame->data, frame->linesize, 0, height, dstData, dstLinesize);
                     img.UnlockBits(bitmapData);

                     OnFrameReceived?.Invoke(this, img);
                  }
               }
            }
            ffmpeg.av_packet_unref(packet);
         }

         ffmpeg.sws_freeContext(swsContext);
         ffmpeg.av_packet_free(&packet);
         ffmpeg.av_frame_free(&frame);
      }
      #endregion
   }
}
