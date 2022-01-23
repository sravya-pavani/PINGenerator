using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using PINGenerator.Models;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace PINGenerator.Controllers
{
    public class PINController : Controller
    {
        // specify the file name to store generated pins.
        string fileName = @"\Inject.data";
        FileStream stream = null;

        #region View Methods
        /// <summary>
        /// Index this instance.
        /// </summary>
        /// <returns>The index.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Generates the pin.
        /// </summary>
        /// <returns>The pin.</returns>
        [HttpGet]
        public JsonResult GeneratePin(int number)
        {
            //min and max values between 100 and 9999 for the pin.
            int min = 100;
            int max = 9999;
            int randomPin = 0000;

            //declare a list of pinmodel to return.
            List<PINModel> pinModels = new List<PINModel>();
            try
            {
                //generates random number between the specified limit.
                Random _rdm = new Random();
                for (int i = 0; i < number; i++)
                {
                    randomPin = _rdm.Next(min, max);

                    //checks the pin generated under various rules and
                    //checks the pin is already generated to avoid duplicate pins
                    //if any of the cases got failed, then generating a new pin
                    //else, generated pin is stored in the list.
                    if (!CheckValidStatusOfPin(randomPin) || !CheckPinBeforeInsertion(randomPin))
                        i--;
                    else
                    {
                        pinModels.Add(new PINModel() { pinInNum = randomPin });
                    }
                }

                //calls the method to save generated pins to a file
                PersistDataInAFile(pinModels);
                return Json(pinModels, JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Emit the pin(s).
        /// </summary>
        /// <returns>The pin.</returns>
        [HttpGet]
        public JsonResult EmitPin()
        {
            //Emit PIN(s)
            try
            {
                //checks if file exists 
                //if so, if the generated pin exists already in the file.
                if (CheckFileExistsAndDataExixtsInFile())
                {
                    //calls a method to read data from the file
                    List<PINModel> pinModels = ReadPinsFromFile();
                    return Json(pinModels, JsonRequestBehavior.AllowGet);
                }
                else
                    return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Check if file exists
        /// <summary>
        /// Checks the file exists and data exixts in file.
        /// </summary>
        /// <returns><c>false</c>, if file exists and data exixts in file was checked, <c>true</c> otherwise.</returns>
        public bool CheckFileExistsAndDataExixtsInFile()
        {
            bool flag = true;
            FileInfo fileInfo = new FileInfo(fileName);

            try
            {
                //check if file exists.
                if (fileInfo.Exists)
                {
                    //checks if file has data
                    if (new FileInfo(fileName).Length == 0)
                    {
                        flag = false;
                    }
                }
                else
                {
                    flag = false;
                }
                return flag;
            }
            catch (Exception ex)
            {
                return false;
            }           
        }
        #endregion

        #region Check Pin if already generated
        /// <summary>
        /// Checks the pin before insertion.
        /// </summary>
        /// <returns><c>true</c>, if pin before insertion was checked, <c>false</c> otherwise.</returns>
        /// <param name="pin">Pin.</param>
        public bool CheckPinBeforeInsertion(int pin)
        {
            bool flag = true;
            FileInfo fileInfo = new FileInfo(fileName);
            try
            {
                if (fileInfo.Exists)
                {
                    if (new FileInfo(fileName).Length == 0)
                    {
                        List<PINModel> pinModels = ReadPinsFromFile();
                        foreach (var item in pinModels)
                        {
                            if (item.pinInNum == pin)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Persist data in a file
        /// <summary>
        /// Persists the data in AF ile.
        /// </summary>
        /// <param name="pinModels">Pin models.</param>
        public void PersistDataInAFile(List<PINModel> pinModels)
        {
            // Create a StreamWriter from FileStream  
            // Check to see if the file exists.
            FileInfo fileInfo = new FileInfo(fileName);

            if (fileInfo.Exists)
            {
                if (new FileInfo(fileName).Length == 0)
                {
                    //creates the file, and writes the data to the file in separate lines.
                    stream = new FileStream(fileName, FileMode.OpenOrCreate);
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        foreach (var s in pinModels)
                            writer.WriteLine(s.pinInNum);
                    }
                }
                else
                {
                    //appends the data to the existing file.
                    stream = new FileStream(fileName, FileMode.Append, FileAccess.Write);
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        foreach (var s in pinModels)
                            writer.WriteLine(s.pinInNum);
                    }
                }
            }
            else 
            {
                stream = new FileStream(fileName, FileMode.OpenOrCreate);
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    foreach (var s in pinModels)
                        writer.WriteLine(s.pinInNum);
                }
            }
        }

        /// <summary>
        /// Reads the pins from file and removes the file after closing it.
        /// </summary>
        /// <returns>The pins from file.</returns>
        public List<PINModel> ReadPinsFromFile()
        {
            List<PINModel> objnew = new List<PINModel>();
            String line;
            stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            StreamReader sr = new StreamReader(stream);
            try
            {
                //Read the first line of text
                line = sr.ReadLine();

                while (line != null)
                {
                    int pinInNum = Convert.ToInt32(line);
                    objnew.Add(new PINModel() { pinInNum = pinInNum });
                    line = sr.ReadLine();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                sr.Close();
            }
            return objnew;
        }

        #endregion

        #region PIN rules check
        /// <summary>
        /// Checks the valid status of pin.
        /// </summary>
        /// <returns><c>true</c>, if valid status of pin was checked, <c>false</c> otherwise.</returns>
        /// <param name="randomPin">Random pin.</param>
        public bool CheckValidStatusOfPin(int randomPin)
        {
            bool flag = false;
            string strPin = Convert.ToString(randomPin);
            try
            {
                //if pin is equal to current year, then return false.
                if (strPin.Equals("2022"))
                    flag = false;
                else
                {
                    //if pin length is equal to 3 digits, append 0 to the number to make it as 4 digit pin.
                    if (strPin.Length == 3)
                        strPin.Insert(0, "0");
                    if (strPin.Length == 4)
                    {
                        //if pin length is equal to 4 digits, checks two more rules like,
                        //whether the pin contains all 4 consecutive numbers like 1234 (which is not obvious for a pin)
                        //and, whether the pin contains atleast two different digits.
                        if (isConsecutive(strPin) == -1 || maxRepeating(strPin) == -1)
                            flag = false;
                        else
                            flag = true;
                    }
                }
                return flag;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// function to check whether the number contains all consecutive numbers.
        /// </summary>
        /// <returns><c>1</c>if String doesn't contains consecutive numbers, <c>-1</c> otherwise.</returns></returns>
        /// <param name="str">String.</param>
        static int isConsecutive(String str)
        {
            // variable to store starting number
            int startNum;
            int len = str.Length;

            try
            {
                for (int i = 0; i < len / 2; i++)
                {
                    // new String containing the starting
                    // substring of input String
                    String newStr = str.Substring(0, i + 1);

                    // converting starting substring into number
                    int num = int.Parse(newStr);

                    // backing up the starting number in startNum
                    startNum = num;

                    // while loop until the new_String is
                    // smaller than input String
                    while (newStr.Length < len)
                    {
                        num++;
                        // concatenate the next number
                        newStr = newStr + String.Join("", num);
                    }
                    // check if new String becomes equal to input String
                    if (newStr.Equals(str))
                        return -1;
                }
                // if String doesn't contains consecutive numbers
                return 1;
            }
            catch(Exception ex)
            {
                return -1; 
            }
        }

        /// <summary>
        /// function to see how many different numbers in the pin.
        /// </summary>
        /// <returns><c>1</c>if String contains three different numbers, <c>-1</c> otherwise.</returns></returns></returns>
        /// <param name="str">String.</param>
        static int maxRepeating(string str)
        {
            int len = str.Length;
            char res = str[0];
            try
            {
                for (int i = 0; i < len; i++)
                {
                    int cur_count = 1;
                    for (int j = i + 1; j < len; j++)
                    {
                        if (str[i] != str[j])
                            break;
                        cur_count++;
                    }
                    if (cur_count > 2)
                        return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
        #endregion
    }
}