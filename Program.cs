using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace hiresopt {
    internal class Program {

        static int swapNibbles(int x) {
            return ((x & 0x0F) << 4 | (x & 0xF0) >> 4);
        }

        static void Main(string[] args) {

            try {

                string appPath = AppDomain.CurrentDomain.BaseDirectory;

                string[] dirs1 = Directory.GetFiles(appPath, "*.map");
                Console.WriteLine("Found {0} MAP files.", dirs1.Length);
                foreach (string dir in dirs1) {
                    Console.WriteLine(dir);
                }

                string[] dirs2 = Directory.GetFiles(appPath, "*.scr");
                Console.WriteLine("Found {0} SCR files.", dirs2.Length);
                foreach (string dir in dirs2) {
                    Console.WriteLine(dir);
                }

                if (dirs1.Length > 0 && dirs2.Length > 0) {

                    for (int f = 0; f < dirs1.Length && f < dirs2.Length; f++) {

                        // ========================================================================
                        Console.WriteLine();
                        Console.WriteLine("Processing files: {0}, {1}", dirs1[f], dirs2[f]);

                        var map = File.ReadAllBytes(dirs1[f]);
                        var scr = File.ReadAllBytes(dirs2[f]);

                        var sizey = scr.Length / 40;
                        var sizex = scr.Length / 25;

                        byte[] data = new byte[256];

                        foreach (byte b in scr) {
                            data[b] = b;
                        }

                        // ========================================================================
                        Console.WriteLine();
                        Console.WriteLine("All nibbles:");
                        foreach (byte b in data) {
                            if (b != 0) {
                                Console.Write("{0:X} ", b);
                            }
                        }

                        // ========================================================================
                        Console.WriteLine();
                        Console.WriteLine("1) Optimizing nibbles:");

                        for (int i = 0; i < 0x80; i++) {

                            var i2 = swapNibbles(i);

                            if (data[i] != 0 && data[i2] != 0) {
                                Console.WriteLine("{0:X} --> {1:X}", data[i], data[i2]);

                                for (int s = 0; s < scr.Length; s++) {
                                    if (scr[s] == data[i]) {
                                        scr[s] = data[i2];
                                        for (int j = 0; j < 8; j++) {
                                            map[s * 8 + j] ^= 0xff;
                                        }
                                    }
                                }

                            }
                        }

                        // ========================================================================

                        Console.WriteLine();
                        Console.WriteLine("2) Optimizing 0xFFs:");

                        for (int i = 0; i < 0x100; i++) {

                            int foundHigh = 0;
                            int foundLow = 0;

                            if (data[i] != 0) {

                                for (int s = 0; s < scr.Length; s++) {
                                    if (scr[s] == data[i]) {

                                        int found = 0;
                                        for (int j = 0; j < 8; j++) {

                                            if (map[s * 8 + j] > 0x80) {
                                                found++;
                                            }
                                        }

                                        if (found > 4) {
                                            foundHigh++;
                                        } else {
                                            foundLow++;
                                        }

                                    }
                                }

                                if (foundHigh > foundLow) {
                                    Console.Write("{0:X} ", data[i]);
                                    Console.WriteLine("Found {0} vs {1} - to be optimized.", foundHigh, foundLow);

                                    for (int s = 0; s < scr.Length; s++) {
                                        if (scr[s] == i) {

                                            scr[s] = (byte)swapNibbles(scr[s]);
                                            for (int j = 0; j < 8; j++) {
                                                map[s * 8 + j] ^= 0xff;
                                            }
                                        }
                                    }
                                } else {
                                    //Console.WriteLine("Found {0} vs {1} not to be optimized.", foundHigh, foundLow);
                                }

                            }

                        }

                        // ========================================================================
                        //Console.WriteLine();
                        //Console.WriteLine("3) Optimizing BMP:");

                        //for (int i = 0x80; i < 0x100; i++) {

                        //    var newbyte = (byte)swapNibbles(i);

                        //    if (data[i] != 0 && data[newbyte] == 0) {
                        //        Console.Write("{0:X} ", data[i]);

                        //        for (int s = 0; s < scr.Length; s++) {
                        //            if (scr[s] == data[i]) {

                        //                int found = 0;
                        //                for (int j = 0; j < 8; j++) {
                        //                    if (map[s * 8 + j] > 0x80) {
                        //                        found++;
                        //                    }
                        //                }
                        //                if (found > 4) {
                        //                    Console.Write(".");

                        //                    scr[s] = newbyte;
                        //                    for (int j = 0; j < 8; j++) {
                        //                        map[s * 8 + j] ^= 0xff;
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        Console.WriteLine();
                        //    }

                        //}

                        // ========================================================================
                        Console.WriteLine();
                        Console.WriteLine("Writing output files.");

                        Directory.CreateDirectory(Path.GetDirectoryName(appPath) + "\\out\\");
                        File.WriteAllBytes(Path.GetDirectoryName(dirs1[f]) + "\\out\\" + Path.GetFileName(dirs1[f]), map);
                        File.WriteAllBytes(Path.GetDirectoryName(dirs2[f]) + "\\out\\" + Path.GetFileName(dirs2[f]), scr);

                        // ========================================================================
                    }

                }

                Console.ReadKey();

            } catch (Exception e) {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

        }
    }
}
