﻿using DataManagement.Standard.Models.Test;
using DataManagement.Standard.Tools;
using DataManagement.Standard.Tools.Test;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement.Standard.UnitTests
{
    [TestFixture]
    class FileSerializerTests
    {
        [Test, Order(1)]
        public void SerializeListOfTypeToFile_FolderExist_ReturnsNoError()
        {
            Directory.CreateDirectory(TestTools.TestDirectory);

            Assert.DoesNotThrow(() => FileSerializer.SerializeListOfTypeToFile(TestTools.ListTestModel, TestTools.TestDirectory + TestTools.TextFileName, '|'));
        }

        [Test]
        public void SerializeListOfTypeToFile_FolderDoesNotExist_ReturnsError()
        {
            Assert.Throws<DirectoryNotFoundException>(() => FileSerializer.SerializeListOfTypeToFile(TestTools.ListTestModel, TestTools.RandomDirectory + TestTools.TextRandomFileName, '|'));
        }

        [Test, Order(2)]
        public void DeserializeFileToListOfType_FileExist_ReturnsNoError()
        {
            Assert.DoesNotThrow(() => FileSerializer.DeserializeFileToListOfType<TestModel>(TestTools.TestDirectory + TestTools.TextFileName, '|', Encoding.UTF7));
        }

        [Test]
        public void DeserializeFileToListOfType_FileDoesNotExist_ReturnsError()
        {
            Assert.Throws<DirectoryNotFoundException>(() => FileSerializer.DeserializeFileToListOfType<TestModel>(TestTools.RandomDirectory + TestTools.TextRandomFileName, '|', Encoding.UTF7));
        }
    }
}