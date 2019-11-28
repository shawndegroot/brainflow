﻿using System;
using brainflow;

using Accord.Math;

namespace test
{
    class GetBoardData
    {
        static void Main (string[] args)
        {
            // use synthetic board for demo
            BoardShim.enable_dev_board_logger ();
            BrainFlowInputParams input_params = new BrainFlowInputParams ();
            int board_id = (int)BoardIds.SYNTHETIC_BOARD;

            BoardShim board_shim = new BoardShim (board_id, input_params);
            board_shim.prepare_session ();
            board_shim.start_stream (3600);
            BoardShim.log_message ((int)LogLevels.LEVEL_INFO, "Start sleeping in the main thread");
            System.Threading.Thread.Sleep (5000);
            board_shim.stop_stream ();
            Console.WriteLine ("data count: {0}", board_shim.get_board_data_count ());
            double[,] unprocessed_data = board_shim.get_current_board_data (64);
            int[] eeg_channels = BoardShim.get_eeg_channels (board_id);
            board_shim.release_session ();

            for (int i = 0; i < eeg_channels.Length; i++)
            {
                Console.WriteLine ("Original data:");
                Console.WriteLine ("[{0}]", string.Join (", ", unprocessed_data.GetRow (eeg_channels[i])));
                // demo for wavelet transform
                // tuple of coeffs array in format[A(J) D(J) D(J-1) ..... D(1)] where J is a
                // decomposition level, A - app coeffs, D - detailed coeffs, and array which stores
                // length for each block, len of this array is decomposition_length + 1
                Tuple<double[], int[]> wavelet_data = DataFilter.perform_wavelet_transform(unprocessed_data.GetRow (eeg_channels[i]), "db4", 3);
                // print app coeffs
                for (int j = 0; j < wavelet_data.Item2[0]; j++)
                {
                    Console.Write (wavelet_data.Item1[j] + " ");
                }
                Console.WriteLine ();
                // you can do smth with wavelet coeffs here, for example denoising works via thresholds for wavelets coeffs
                double[] restored_data = DataFilter.perform_inverse_wavelet_transform (wavelet_data, unprocessed_data.GetRow (eeg_channels[i]).Length, "db4", 3);
                Console.WriteLine ("Restored data:");
                Console.WriteLine ("[{0}]", string.Join (", ", restored_data));
            }
        }
    }
}