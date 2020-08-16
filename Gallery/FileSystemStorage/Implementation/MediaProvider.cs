﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using FileSystemStorage.Interfaces;

namespace FileSystemStorage.Implementation
{
    public class MediaProvider : IMediaProvider
    {
        private readonly IFile _file;

        public MediaProvider(IFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        public bool Upload(byte[] dateBytes, string path)
        {
            if (dateBytes == null)
                throw new ArgumentNullException(nameof(dateBytes));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Argument_EmptyPath", nameof(path));
            var file = new FileSystem();
            _file.WriteAllBytes(path, dateBytes);
            return _file.Exists(path);
        }

        public byte[] Read(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Argument_EmptyPath", nameof(path));

            return _file.ReadAllBytes(path);
        }

        public bool Delete(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Argument_EmptyPath", nameof(path));

            _file.Delete(path);
            return _file.Exists(path);
        }
    }
}
