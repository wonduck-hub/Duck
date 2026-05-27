using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DMedia
{
    public static unsafe class FFmpegFunctionChecker
    {
        /// <summary>
        ///     FFmpeg 함수가 정상적으로 호출되는지 확인하기 위한 전역 Class
        /// </summary>
        /// <exception cref="ApplicationException"></exception>
        public static bool ValidateCoreFunctions()
        {
            try
            {
                bool result = true;
                Debug.WriteLine("FFmpeg 함수 로드 검증 시작...");

                // 메모리 할당 테스트
                var ptr = ffmpeg.av_malloc(64);
                if (ptr == null)
                {
                    result = false;
                }
                ffmpeg.av_free(ptr);

                // 컨텍스트 할당 테스트
                var fmtCtx = ffmpeg.avformat_alloc_context();
                if (fmtCtx == null)
                {
                    result = false;
                }
                ffmpeg.avformat_free_context(fmtCtx);

                // 코덱 검색 테스트
                var codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_VP8);
                if (codec == null)
                {
                    result = false;
                }

                // 프레임 할당 테스트
                var frame = ffmpeg.av_frame_alloc();
                if (frame == null)
                {
                    result = false;
                }
                ffmpeg.av_frame_free(&frame);

                // 패킷 할당 테스트
                var pkt = ffmpeg.av_packet_alloc();
                if (pkt == null)
                {
                    result = false;
                }
                ffmpeg.av_packet_free(&pkt);

                // SWS 컨텍스트 테스트
                var swsCtx = ffmpeg.sws_getContext(
                    320, 240, AVPixelFormat.AV_PIX_FMT_BGR24,
                    320, 240, AVPixelFormat.AV_PIX_FMT_YUV420P,
                    (int)SwsFlags.SWS_FAST_BILINEAR, null, null, null);
                if (swsCtx == null)
                {
                    result = false;
                }
                ffmpeg.sws_freeContext(swsCtx);

                Debug.WriteLine("FFmpeg 주요 함수 로딩 및 호출 성공");

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"FFmpeg 초기화 실패\n{ex}");
                return false;
            }
        }
    }
}
