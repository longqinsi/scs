using System;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Communication.Messages;
using Hik.Communication.ScsServices.Client;
using PhoneBookCommonLib;

/* This is a simple client application that uses phone book server.
 */

namespace PhoneBookClient
{
    class Program
    {
        static void Main()
        {
            var messageClient = ScsClientFactory.CreateClient(new ScsTcpEndPoint("127.0.0.1", Consts.ServerPort));
            messageClient.MessageReceived += messageClient_MessageReceived;
            //Create a client to connecto to phone book service on local server and 10048 TCP port.
            var phoneBookServiceClient = ScsServiceClientBuilder.CreateClient<IPhoneBookService>(messageClient);
            var calculatorServiceClient = ScsServiceClientBuilder.CreateClient<ICalculatorService>(messageClient);
            var singleCallServiceClient = ScsServiceClientBuilder.CreateClient<ISingleCallService>(messageClient);
            var singletonServiceClient = ScsServiceClientBuilder.CreateClient<ISingletonService>(messageClient);
            var itemPropertyServiceClient = ScsServiceClientBuilder.CreateClient<ItemPropertyServiceBase>(messageClient);
            Console.WriteLine("Press enter to connect to phone book service...");
            Console.ReadLine();

            Console.WriteLine("itemPropertyServiceClient.ServiceProxy[5]:" + itemPropertyServiceClient.ServiceProxy[5]);
            itemPropertyServiceClient.ServiceProxy[6] = 7;
            Console.WriteLine(phoneBookServiceClient.ServiceProxy.ServiceName);

            var person1 = new PhoneBookRecord { Name = "Halil ibrahim", Phone = "5881112233", Age = 35 };
            var person2 = new PhoneBookRecord { Name = "John Nash", Phone = "58833322211", Age = null };


            //Add the first person
            phoneBookServiceClient.ServiceProxy.AddPerson(person1);

            double doubleA = 4;
            double doubleB = 5;
            double productDouble = calculatorServiceClient.ServiceProxy.Multiply(doubleA, doubleB);
            Console.WriteLine("double: {0}*{1}={2}", doubleA, doubleB, productDouble);
            var origDoubleA = doubleA;
            calculatorServiceClient.ServiceProxy.Add(ref doubleA, doubleB);
            Console.WriteLine("double: {0}+{1}={2}", origDoubleA, doubleB, doubleA);

            Console.WriteLine("log({0})={1}", doubleA, calculatorServiceClient.ServiceProxy.Log(doubleA));
            Console.WriteLine("log({0},{1})={2}", doubleA, doubleB, calculatorServiceClient.ServiceProxy.Log(doubleA, doubleB));

            int intA = 4;
            int intB = 5;
            int productInt = calculatorServiceClient.ServiceProxy.Multiply(intA, intB);
            Console.WriteLine("int: {0}*{1}={2}", intA, intB, productInt);

            calculatorServiceClient.ServiceProxy.Devide(5.0, 2.0);

            messageClient.SendMessage(new ScsTextMessage("Text message from client."));

            double[] array = new double[] { 4, 5.5, -3, Math.PI };
            string exp;
            double sum;
            calculatorServiceClient.ServiceProxy.Add(out sum, out exp, 4, 5.5, -3, Math.PI);
            Console.WriteLine(exp.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries)[0] + " = " + sum);

            //Add the second person
            phoneBookServiceClient.ServiceProxy.AddPerson(person2);

            //Search for a person
            var person = phoneBookServiceClient.ServiceProxy.FindPerson("John");
            if (person != null)
            {
                Console.WriteLine("Person is found:");
                Console.WriteLine(person);
            }
            else
            {
                Console.WriteLine("Can not find person!");
            }

            person2.Age = 30;

            //Update the second person
            var person2backup = person2;
            Console.WriteLine("Update result:" + phoneBookServiceClient.ServiceProxy.UpdatePerson(ref person2));
            Console.WriteLine("The second person:" + person2);
            Console.WriteLine("ReferencEquals:" + object.ReferenceEquals(person2backup, person2));

            PhoneBookRecord person3 = null;
            phoneBookServiceClient.ServiceProxy.UpdatePerson(ref person3);
            Console.WriteLine("person3 is null:" + (person3 == null));

            for (int i = 0, counter = singleCallServiceClient.ServiceProxy.Increment(); i < 10; i++, counter = singleCallServiceClient.ServiceProxy.Increment())
            {
                Console.WriteLine("Single call:i={0},counter={1}", i + 1, counter);
            }

            for (int i = 0, counter = singletonServiceClient.ServiceProxy.Increment(); i < 10; i++, counter = singletonServiceClient.ServiceProxy.Increment())
            {
                Console.WriteLine("Singlton:i={0},counter={1}", i + 1, counter);
            }

            for (int i = 0, counter = singleCallServiceClient.ServiceProxy.Count; i < 10; i++, counter = singleCallServiceClient.ServiceProxy.Count)
            {
                Console.WriteLine("Single call:i={0},counter={1}", i + 1, counter);
            }

            for (int i = 0, counter = singletonServiceClient.ServiceProxy.Count; i < 10; i++, counter = singletonServiceClient.ServiceProxy.Count)
            {
                Console.WriteLine("Singlton:i={0},counter={1}", i + 1, counter);
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to disconnect from phone book service...");
            Console.ReadLine();

            ////Disconnect from server
            //phoneBookServiceClient.Unregister();
            //calculatorServiceClient.Unregister();
            messageClient.Disconnect();
        }

        static void messageClient_MessageReceived(object sender, Hik.Communication.Scs.Communication.Messages.MessageEventArgs e)
        {
            var textMessage = e.Message as ScsTextMessage;
            if(textMessage != null)
            {
                Console.WriteLine(textMessage.Text);
            }
        }
    }
}
