using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IoCContainerDemo
{
    class Program
    {
        /// <summary>The shopper class makes use of an interface ICreditcard to reduce 
        /// dependencies on it's concrete class. The ICreditcard holds a method as a maember
        /// This method is defined in shopper but when defined the paramteter takes an Icreditcard
        /// as a property, therefore allowing all instances of shopper to call the charge method
        /// but only if this instance that shopper takes as a parameter inherits from an Icreditcard
        /// This means that each instance of shopper can be used interchangeably as it uses
        /// An interface to do it's work as opposed to base implementations.
        /// </summary> the dependency on 
        static void Main(string[] args)
        {
            Resolver resolver = new Resolver();
            //shopper not dependent on mastercard, dependent on interface.
            //setting up the rules below of how
            resolver.Register<Shopper, Shopper>();
            resolver.Register<ICreditCard, Mastercard>(); //Dictionary is being populated
            //var shopper1 = new Shopper(IApplicationRe); //shopper class can be used interchangeably.
            var shopper = resolver.Resolve<Shopper>();

            shopper.Charge();
            //shopper1.Charge();
            Console.Read();
        }
    }

    public class Resolver
    {
        private Dictionary<Type, Type> dependencyMap = new Dictionary<Type, Type>();

        public T Resolve<T>() //calling resolve method
        {
            return (T) Resolve(typeof (T)); //syntatic sugar which allows you to pass any type into resolve
        }

        private object Resolve(Type typeToResolve)
            //this type you want, let's see if is in the dictionary, not the class but the type that the class is.
        {
            Type resolvedType = null;
            try
            {
                resolvedType = dependencyMap[typeToResolve]; //look in dictionary to see if type to resolve is there.
            }
            catch (Exception) //if not in dictionary
            {
                throw new Exception(string.Format("Could not resolve type {0}", typeToResolve.FullName));
            }

            //then, look at the type's first constructor, and look to see if it has prams, we know that it doesn't
            var firstConstructor = resolvedType.GetConstructors().First();
            var constructorParameters = firstConstructor.GetParameters(); //then get those parameters of the typetoResolve passsed in

            if (constructorParameters.Count() == 0)
                return Activator.CreateInstance(resolvedType);
            //like newing up except calling it against a type. creates a parameterless constructor for a type.

            //make alist to hold the parameters we resolve

            IList<object> parameters = new List<object>();
            foreach (var parameterToResolve in constructorParameters)
            {
                parameters.Add(Resolve(parameterToResolve.ParameterType));
            } // foreach parameter to resolve as we get the parameters, add the parameter to resolve to the list by type

            return firstConstructor.Invoke(parameters.ToArray());
             //we return the list of parameters using invoke. When object is created, list is returned.
        }

        public void Register<TFrom, TTo>()
        {
           dependencyMap.Add(typeof(TFrom), typeof(TTo));
            //adding to our dictionary, mapping shopper against shopper
            //type of gets the type of whatever is passed in, using reflection
        }
    }

public class visa : ICreditCard
        {
            public string Charge()
            {
                return "Charging with the visa";
            }
        }

        public class Mastercard : ICreditCard
        {
            public string Charge()
            {
                return "Swiping the Mastercard";
            }
        }

        public class Shopper
        {
            private readonly ICreditCard _creditcard;

            public Shopper(ICreditCard creditcard)
            {
                _creditcard = creditcard;
            }

            public void Charge()
            {
                var chargeMessage = _creditcard.Charge();
                Console.WriteLine(chargeMessage);
            }
        }

        public interface ICreditCard
        {
            string Charge();
        }

    }

