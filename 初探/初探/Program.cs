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

            //暫存是否新產生的參數是可以使用的
            bool Module_OK = true;

            //輸入參數 10樣本
            int[] input = new int[10];


            //存放運算結果
            float answer = 0;
            //製造一個假定的數值
            for (int i = 0; i < input.Length; i++) input[i] = ran.Next(0, 2);
            for (int i = 0; i < input.Length; i++)
                answer += input[i] * origin[i];

            //寫新檔案
            StreamWriter sw_input = new StreamWriter(path + @"\input\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt");
            for (int i = 0; i < input.Length; i++) sw_input.WriteLine(input[i]);
            sw_input.Close();
            sw_input.Dispose();

            //紀錄
            DateTime Start = DateTime.Now;
            
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
                            if (learn_origin[i] > 1 || learn_origin[i] < 0) i--;
                            sr_inner.Close();
                            sr_inner.Dispose();
                        }

                        for (int i = 0; i < learn_origin.Length; i++)
                            learn_answer += input[i] * learn_origin[i];

                        //計算準確率(越靠近1越準確)
                        float difference = (answer > learn_answer) ? learn_answer / answer : answer / learn_answer;

                        if (pre_difference < difference){
                            //先重置Module訊號
                            Module_OK = true;
                            //檢查先前所有模型資料是否符合本次修改結果
                            //查詢目錄
                            string[] dirs = Directory.GetFiles(path + @"input\");
                            for(int i = 0; i < dirs.Length; i++){
                                float path_answer = 0;
                                float[] path_input = new float[10];
                                StreamReader sr_path = new StreamReader(dirs[i]);
                                for (int k = 0; k < 10; k++) path_input[k] = float.Parse(sr_path.ReadLine());
                                sr_path.Close();
                                sr_path.Dispose();
                                for (int k = 0; k < path_input.Length; k++)
                                    path_answer += path_input[k] * learn_origin[k];
                                float path_difference = (answer > path_answer) ? path_answer / answer : answer / path_answer;
                                //如果反而造成準確度下降則不採用新的參數
                                if (pre_difference > path_difference){
                                    Module_OK = false;
                                    break;
                                }
                            }

                            if (Module_OK == false) break;

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

                //如果前次>本次 (表示準確度提升)
                if (Module_OK == true)
                    Console.WriteLine(string.Format("第\t{0}\t次模型準確度：{1} %", max + 1, pre_difference * 100));
                //超過95%就當作完成
                if (pre_difference >= 0.95f) break;
                max++;
            }

            Console.WriteLine(string.Format("\n------------------------------------\n{0} 完成模型準確度95%建置\n費時：{1}\n" +
                "即將開始下一次模型建置\n------------------------------------\n", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss "), (DateTime.Now - Start).ToString()));

            //該次判斷結束，重新再生成數字一次
            Main(null);

            Console.ReadKey();
        }
    }
}
