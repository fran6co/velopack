﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace Squirrel.Locators
{
    /// <summary>
    /// Provides a mock / test implementation of <see cref="SquirrelLocator" />. This can be used to verify that
    /// your application is able to find and prepare updates from your chosen update source without actually
    /// having an installed Squirrel application. This could be used in a CI/CD pipeline, or unit tests etc.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestSquirrelLocator : SquirrelLocator
    {
        /// <inheritdoc />
        public override string AppId {
            get {
                if (_id == null) {
                    throw new NotSupportedException("AppId is not supported in this test implementation.");
                }
                return _id;
            }
        }

        /// <inheritdoc />
        public override string RootAppDir {
            get {
                if (_root == null) {
                    throw new NotSupportedException("RootAppDir is not supported in this test implementation.");
                }
                return _root;
            }
        }

        /// <inheritdoc />
        public override string PackagesDir {
            get {
                if (_packages == null) {
                    throw new NotSupportedException("PackagesDir is not supported in this test implementation.");
                }
                return _packages;
            }
        }

        /// <inheritdoc />
        public override string UpdateExePath {
            get {
                if (_updatePath == null) {
                    throw new NotSupportedException("UpdateExePath is not supported in this test implementation.");
                }
                return _updatePath;
            }
        }

        /// <inheritdoc />
        public override SemanticVersion CurrentlyInstalledVersion {
            get {
                if (_version == null) {
                    throw new NotSupportedException("CurrentlyInstalledVersion is not supported in this test implementation.");
                }
                return _version;
            }
        }
        /// <inheritdoc />
        public override string AppContentDir {
            get {
                if (_appContent == null) {
                    throw new NotSupportedException("AppContentDir is not supported in this test implementation.");
                }
                return _appContent;
            }
        }

        private readonly string _updatePath;
        private readonly SemanticVersion _version;
        private readonly string _packages;
        private readonly string _id;
        private readonly string _root;
        private readonly string _appContent;

        /// <inheritdoc cref="TestSquirrelLocator" />
        public TestSquirrelLocator(string appId, string version, string packagesDir, ILogger logger = null)
            : this(appId, version, packagesDir, null, null, null, logger)
        {
        }

        /// <inheritdoc cref="TestSquirrelLocator" />
        public TestSquirrelLocator(string appId, string version, string packagesDir, string appDir,
            string rootDir, string updateExe, ILogger logger = null)
            : base(logger)
        {
            _id = appId;
            _packages = packagesDir;
            _version = SemanticVersion.Parse(version);
            _updatePath = updateExe;
            _root = rootDir;
            _appContent = appDir;
        }
    }
}
