﻿using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gameframe.SaveLoad.Tests
{
    public class SaveLoadManagerTests
    {

        [Serializable]
        public class SaveLoadTestObject
        {
            public List<string> listOfStrings = new List<string>();
            public int count;
        }

        public class SaveLoadTestUnityObject : ScriptableObject
        {
            public string textValue = "";
            public Vector3 pt;
            public Quaternion rot;
        }
        
        private static readonly string BaseDirectory = "GameData";
        private static readonly string SaveDirectory = "SaveData";
        private static readonly string TestEncryptionKey = "SaveLoadTestEncryptionKey";
        private static readonly string TestEncryptionSalt = "SaveLoadTestEncryptionSalt";

        private static SaveLoadManager CreateManager(SerializationMethodType method = SerializationMethodType.Default)
        {
            var manager = SaveLoadManager.Create(BaseDirectory,SaveDirectory,method,TestEncryptionKey, TestEncryptionSalt);
            if (method == SerializationMethodType.Custom)
            {
                manager.SetCustomSerializationMethod(new SerializationMethodUnityJson());
            }
            return manager;
        }

        [Test]
        public void CanCreateManager([Values] SerializationMethodType method)
        {
            var manager = CreateManager(method);
            Assert.IsTrue(manager != null);
            Object.Destroy(manager);
        }

        [Test]
        public void CanSave([Values] SerializationMethodType method)
        {
            var manager = CreateManager(method);

            var testObject = new SaveLoadTestObject()
            {
                listOfStrings = new List<string> {"one", "two"},
                count = 10,
            };

            const string filename = "Testfile";
            
            manager.Save(testObject,filename);

            var filepath = $"{SaveLoadUtility.GetSavePath(SaveDirectory, BaseDirectory)}/{filename}";
            Assert.IsTrue(File.Exists(filepath));
            
            manager.DeleteSave(filename);
            Assert.IsFalse(File.Exists(filepath));
            
            Object.Destroy(manager);
        }
        
        [Test]
        public void CanSaveAndLoad([Values] SerializationMethodType method)
        {
            var manager = CreateManager(method);

            var testObject = new SaveLoadTestObject()
            {
                listOfStrings = new List<string> {"one", "two"},
                count = 10,
            };

            const string filename = "Testfile";
            
            manager.Save(testObject,filename);

            var filepath = $"{SaveLoadUtility.GetSavePath(SaveDirectory, BaseDirectory)}/{filename}";
            Assert.IsTrue(File.Exists(filepath));

            var loadedObject = manager.Load<SaveLoadTestObject>(filename);
            
            Assert.IsTrue(loadedObject.listOfStrings.Count == testObject.listOfStrings.Count);

            for (int i = 0; i < testObject.listOfStrings.Count; i++)
            {
                Assert.IsTrue(testObject.listOfStrings[i] == loadedObject.listOfStrings[i]);
            }
            
            Assert.IsTrue(testObject.count == loadedObject.count);
            
            manager.DeleteSave(filename);
            Assert.IsFalse(File.Exists(filepath));
            
            Object.Destroy(manager);
        }
        
        [Test]
        public void LoadReturnsNullWhenFileDoesnotExist([Values] SerializationMethodType method)
        {
            var manager = CreateManager(method);
            const string filename = "Testfile";

            var loadedObject = manager.Load<SaveLoadTestObject>(filename);
            Assert.IsTrue(loadedObject == null);
            
            Object.Destroy(manager);
        }

        [Test]
        public void CanSaveUnityObject([Values] SerializationMethodType method)
        {
            const string filename = "Testfile";

            var manager = CreateManager(method);
            var testObj = ScriptableObject.CreateInstance<SaveLoadTestUnityObject>();

            testObj.textValue = "MyValue";

            manager.SaveUnityObject(testObj,filename);
           
            var filepath = $"{SaveLoadUtility.GetSavePath(SaveDirectory, BaseDirectory)}/{filename}";
            Assert.IsTrue(File.Exists(filepath));
            
            manager.DeleteSave(filename);
            Assert.IsFalse(File.Exists(filepath));
            
            Object.Destroy(testObj);
            Object.Destroy(manager);
        }
        
        [Test]
        public void CanSaveLoadUnityObject([Values] SerializationMethodType method)
        {
            const string filename = "Testfile";

            var manager = CreateManager(method);
            var testObj = ScriptableObject.CreateInstance<SaveLoadTestUnityObject>();
            var loadedTestObj = ScriptableObject.CreateInstance<SaveLoadTestUnityObject>();

            testObj.textValue = "MyValue";

            manager.SaveUnityObject(testObj,filename);
           
            var filepath = $"{SaveLoadUtility.GetSavePath(SaveDirectory, BaseDirectory)}/{filename}";
            Assert.IsTrue(File.Exists(filepath));

            //var loadedTestObj = manager.Load<SaveLoadTestUnityObject>(filename);
            //Assert.IsTrue(loadedTestObj != null);
            manager.LoadUnityObjectOverwrite(loadedTestObj,filename);
            Assert.IsTrue(loadedTestObj.textValue == testObj.textValue);
            
            manager.DeleteSave(filename);
            Assert.IsFalse(File.Exists(filepath));
            
            Object.Destroy(testObj);
            Object.Destroy(loadedTestObj);
            Object.Destroy(manager);
        }

        
    }
}
