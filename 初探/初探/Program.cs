using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 初探{
    class Program{
        private static string path = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;  //執行檔位置
        static void Main(string[] args){

            //給定的隱藏參數
            float[] origin = new float[10] { 0.93f, 0.46f, 0.58f, 0.7f, 0.12f, 0.37f, 0.62f, 0.41f, 0.2f, 0.25f };
            Random ran = new Random(Guid.NewGuid().GetHashCode());
            float[] var_origin = new float[10];

            int max = 0;    //紀錄總共有幾個樣本數
            int match_count = 0;  //紀錄符合的樣本數量
            float pre_difference = 0;   //前次誤差率

            //輸入參數 10樣本
            int[] input = new int[10];


            //存放運算結果
            float answer = 0;
            //製造一個假定的數值
            for (int i = 0; i < input.Length; i++) input[i] = ran.Next(0, 2);
            for (int i = 0; i < input.Length; i++)
                answer += input[i] * origin[i];


            while (true){

                //讀取學習模型參數
                StreamReader sr = new StreamReader(path + @"var.txt");
                for (int i = 0; i < var_origin.Length; i++) var_origin[i] = float.Parse(sr.ReadLine());
                sr.Close();
                sr.Dispose();

                //運算模型數值
                float tem_answer = 0;
                for (int i = 0; i < input.Length; i++)
                    tem_answer += input[i] * var_origin[i];

                //判斷是否相符
                if (tem_answer.ToString("0.000").Equals(answer.ToString("0.000"))) match_count++;
                else{
                    //自主學習
                    while (true) {
                        //暫存隱藏層參數
                        float[,] inner_origin = new float[10, 10];

                        float[] learn_origin = new float[10];
                        float learn_answer = 0;

                        //計算隱藏層參數
                        for (int i = 0; i < learn_origin.Length; i++){
                            learn_origin[i] = 0;
                            StreamReader sr_inner = new StreamReader(string.Format("{0}var{1}.txt", path, (i + 1).ToString("00")));
                            for (int k = 0; k < 10; k++){
                                float tem_f = float.Parse(sr_inner.ReadLine());
                                //防止亂數數值小於0或大於1
                                do{
                                    inner_origin[i, k] = tem_f + ran.Next(-1, 2) * 0.01f;
                                } while (inner_origin[i, k] < 0 || inner_origin[i, k] > 1);
                                learn_origin[i] += input[k] * inner_origin[i, k];
                            }
                            sr_inner.Close();
                            sr_inner.Dispose();
                        }

                        for (int i = 0; i < learn_origin.Length; i++)
                            learn_answer += input[i] * learn_origin[i];

                        //計算準確率(越靠近1越準確)
                        float difference = (answer > learn_answer) ? learn_answer / answer : answer / learn_answer;

                        if (pre_difference < difference){
                            //改寫隱藏層檔
                            for (int i = 0; i < learn_origin.Length; i++){
                                StreamWriter sw_inner = new StreamWriter(string.Format("{0}var{1}.txt",path, (i + 1).ToString("00")));
                                for (int k = 0; k < 10; k++)
                                    sw_inner.WriteLine(inner_origin[i, k].ToString("0.00"));
                                sw_inner.Close();
                                sw_inner.Dispose();
                            }
                            //改寫輸出var檔
                            StreamWriter sw = new StreamWriter(path + "var.txt");
                            for (int i = 0; i < learn_origin.Length; i++)
                                sw.WriteLine(learn_origin[i].ToString("0.00"));
                            sw.Close();
                            sw.Dispose();
                            pre_difference = difference;
                            break;
                        }
                    }
                }

                Console.WriteLine(string.Format("第\t{0}\t次模型誤差值：{1} %", max, pre_difference * 100));
                if (pre_difference >= 0.99f) break;
                max++;
            }
            
            Console.ReadKey();
        }
    }
}
