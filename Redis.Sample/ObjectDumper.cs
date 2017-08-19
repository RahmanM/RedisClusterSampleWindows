using System;
using System.Xml.Serialization;



namespace ObjectDumper
{
    public sealed class ObjectDumper<T> where T : class
    {

        #region Default Constructor

        private ObjectDumper()
        {

        }

        #endregion

        #region Publics

        public static string Dump(T objectToDump)
        {
            if (objectToDump == null)
            {
                throw new ArgumentNullException("objectToDump");
            }

            var objectToString = ObjectToString(objectToDump);
            return objectToString;
        }

        #endregion

        #region Privates

        private static string ObjectToString(T toSerialize)
        {
            var xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (var textWriter = new System.IO.StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        #endregion

    }
}