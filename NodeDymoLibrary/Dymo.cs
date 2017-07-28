using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Threading.Tasks;
using System.IO;
using DYMO.Label.Framework;
using Microsoft.CSharp;

namespace NodeDymoLib
{
    public class Dymo
    {

        public async Task<IPrinters> Printers(dynamic input)
        {
            IPrinters thePrinters;

            try
            {
                thePrinters = await Task.Run(() => new Printers());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return thePrinters;
        }

        /*
		 *
		 *
		 *
		 *
		 */
        public async Task<object> Print(dynamic jobDetails)
        {
            Console.WriteLine("NodeDymoLibrary: print() called");
            Console.WriteLine(jobDetails.printer);
            //
            // Make sure the required parts exist
            if( !PropertyExists(jobDetails, "printer" ) )
            {
                Console.WriteLine("NodeDymoLibrary: No `printer` parameter");
                throw new System.ArgumentException("'printer' parameter must be defined", "original");
            } else if( !PropertyExists(jobDetails, "labels" ) )
            {
                Console.WriteLine("NodeDymoLibrary: No `labels` parameter");
                throw new System.ArgumentException("'labels' parameter must be defined", "original");
            }
            else
            {
                Console.WriteLine("NodeDymoLibrary: Required base parameters defined");
                Console.WriteLine(jobDetails.printer);
            }

            //
            // Setup the printer and this job
            //ILabelWriterPrinter printer = Framework.GetLabelWriterPrinters()[(String)jobDetails.printer] as ILabelWriterPrinter;
            IPrinter printer;
            try
            {
                printer = Framework.GetPrinters()[jobDetails.printer] as IPrinter;
            }
            catch (Exception ex)
            {
                Console.WriteLine("NodeDymoLibrary: unable to find printer");
                throw ex;
            }

            //
            // Set some settings for this printing
            ILabelWriterPrintParams printParams = new LabelWriterPrintParams();
            printParams.PrintQuality = LabelWriterPrintQuality.BarcodeAndGraphics;
            printParams.JobTitle = "Dymo Labels";
            printParams.Copies = (int)1;
            if( PropertyExists(jobDetails, "jobTitle") )
            {
                Console.WriteLine("NodeDymoLibrary Adding Print Job Title: " + (string)jobDetails.jobTitle);
                printParams.JobTitle = (string)jobDetails.jobTitle;
            }
            if( PropertyExists(jobDetails, "copies" ) )
            {
                Console.WriteLine("NodeDymoLibrary Adding Print Copies: " + (string)jobDetails.copies);
                printParams.Copies = (int)jobDetails.copies;
            }
            // Set some settings for this printing 
            //

            IPrintJob printJob = printer.CreatePrintJob( printParams );
            Console.WriteLine("NodeDymoLibrary Print Job Created");

            //
            // Lets loop over these labels
            IDictionary<string, ILabel> label = new Dictionary<string, ILabel>();
            object[] suppliedLabels = (object[])jobDetails.labels;// Cast JS array as Object (no keys) 
            foreach (IDictionary<string, object> thisLabel in suppliedLabels)
            {
                var i = label.Count.ToString();
                if( !thisLabel.ContainsKey("filename") )
                {
                    Console.WriteLine("Dymo.cs No `labels`[x].`filename` parameter");
                    throw new System.ArgumentException("'labels'.'filename' parameter must be defined for each label", "original");
                }
                if( !File.Exists( (string)thisLabel["filename"] ))
                {
                    Console.WriteLine("Dymo.cs Unable to find label filename: " + (string)thisLabel["filename"]);
                    throw new System.ArgumentException("'labels'.'filename' parameter must point to an existing file", "original");
                }

                Console.WriteLine("NodeDymoLibrary Adding label: " + (string)thisLabel["filename"] );
                label[i] = Label.Open( (string)thisLabel["filename"] );
                if( thisLabel.ContainsKey("fields") )
                {
                    Console.WriteLine("NodeDymoLibrary Setting Field Values");
                    IDictionary<string, object> fields = (IDictionary<string, object>)thisLabel["fields"];
                    foreach (var kv in fields)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value.ToString();
                                label[i].SetObjectText(k, v);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                }

                if( thisLabel.ContainsKey("images") )
                {
                    Console.WriteLine("NodeDymoLibrary Setting Image Values");
                    IDictionary<string, object> images = (IDictionary<string, object>)thisLabel["images"];
                    foreach (var kv in images)
                    {
                        try
                        {
                            var k = kv.Key.ToUpper();
                            var obj = label[i].GetObjectByName(k);
                            if (obj != null)
                            {
                                var v = kv.Value;
                                label[i].SetImagePngData(k, new MemoryStream((byte[])v));
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }


                Console.WriteLine("NodeDymoLibrary Add Label to print job");
                printJob.AddLabel(label[i]);
            }


            Console.WriteLine("NodeDymoLibrary Lets Print the Label/s");
            try
            {
                await Task.Run(() => printJob.Print());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine("NodeDymoLibrary Label/s Printed");
            return true;
        }

        private static bool PropertyExists( dynamic source, string propertyName)
        {
            if (source is ExpandoObject)
                return ((IDictionary < string, object >)source).ContainsKey(propertyName);

            return source.GetType().GetProperty(propertyName) != null;
        }
    }
}
