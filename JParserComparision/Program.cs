

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Runtime.CompilerServices;

namespace JParserComparision
{
    class Program
    {
        static int size;
        static int numit;

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static void Main(string[] args)
        {
            Random rand = new Random();

            while (true)//used to run multiple time easily
            {
                //take in command
                Console.WriteLine("Input next command: (size, numit)");
                string inp = Console.ReadLine();
                try
                {
                    inp = inp.Trim();
                    var temp = inp.Split(','); //split into array
                    size = int.Parse(temp[0]); //try to parse the size
                    numit = int.Parse(temp[1]); //try to parse the number of iterations
                }
                catch (Exception e)
                {
                    return;
                }
                double[] Jsonsum = new double[2]; //used to strore the times returned from runtest
                double[] Datasum = new double[2];
                for (var i = 0; i < numit; i++)
                {
                    var temp = RunTestJSON(); //returns the time of the runs
                    Jsonsum[0] += temp.SERtime;
                    Jsonsum[1] += temp.DEStime;
                    temp = RunTestDataContract(); //returns the time of the runs
                    Datasum[0] += temp.SERtime;
                    Datasum[1] += temp.DEStime;
                }
                Console.WriteLine("JSON.NET Serializing: JSON={0}, Datacountract={1}:", Jsonsum[0] / numit, Datasum[0] / numit); //output
                Console.WriteLine("JSON.NET Deserializing: JSON={0}, Datacountract={1}:", Jsonsum[1] / numit, Datasum[1] / numit);
                Console.WriteLine();
            }

            Console.ReadLine();
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]  //forces off most optimization involving this method (in case it notices we are doing
                                                        //things multiple times)
        static times RunTestJSON()
        {
            SimpleObject test = new SimpleObject(size); //big object, binary tree with size levels (factorial growth)
            string str = JsonConvert.SerializeObject(test); //serialize to json in case of JIT stuff
            Stopwatch sw = new Stopwatch(); //start a timer
            sw.Start();
            str = JsonConvert.SerializeObject(test); //serialize
            sw.Stop();
            double time = sw.Elapsed.TotalMilliseconds;// store tiem
            SimpleObject des = JsonConvert.DeserializeObject<SimpleObject>(str); //deserialize for JIT
            des = null; //reset
            sw = new Stopwatch(); //new timer (could reset)
            sw.Start();
            des = JsonConvert.DeserializeObject<SimpleObject>(str); //deserialize (if no error, successful, not worth a check.)
            sw.Stop(); // stop timer
            return new times(time, sw.Elapsed.TotalMilliseconds); //send back time
        }

        [MethodImpl(MethodImplOptions.NoOptimization)] //see comments above, same method, just with DataContractSerializer
        static times RunTestDataContract()
        {
            SimpleObject test = new SimpleObject(size);
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(SimpleObject));
            Stopwatch sw = new Stopwatch();
            var mstream = new MemoryStream();
            js.WriteObject(mstream, test);
            mstream = new MemoryStream();
            mstream.Position = 0;
            sw.Start();
            js.WriteObject(mstream, test);
            sw.Stop();
            double time = sw.Elapsed.TotalMilliseconds;
            mstream.Position = 0;
            SimpleObject des = (SimpleObject)js.ReadObject(mstream);
            mstream.Position = 0;
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            des = (SimpleObject)js.ReadObject(mstream);
            sw2.Stop();
            return new times(time, sw2.Elapsed.TotalMilliseconds);
        }
    }

    [DataContract]
    class SimpleObject
    {
        [DataMember]
        public long foo;
        [DataMember]
        public long bar;
        [DataMember]
        public SimpleObject sub1;
        [DataMember]
        public SimpleObject sub2;
        public SimpleObject(int n)
        {
            foo = 1234980123849123;
            bar = 213451349581092;
            if (n == 0)
            {
                return;
            }
            sub1 = new SimpleObject(n - 1);
            sub2 = new SimpleObject(n - 1);
        }
    }

    class times
    {
        public double SERtime;
        public double DEStime;

        public times(double SERtime, double DEStime)
        {
            this.SERtime = SERtime;
            this.DEStime = DEStime;
        }
    }
}
