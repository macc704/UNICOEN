﻿#region License

// Copyright (C) 2011 The Unicoen Project
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unicoen.CodeGenerators;
using Unicoen.Languages.Tests;
using Unicoen.ProgramGenerators;
using Unicoen.Tests;
using Unicoen.Utils;

namespace Unicoen.Languages.Java.Tests {
    /// <summary>
    ///   テストに必要なデータを提供します．
    /// </summary>
    public partial class JavaFixture : Fixture {
        private const string MavenArg = "compile";
        private const string CompileCommand = "javac";
        private const string DisassembleCommand = "javap";
        private readonly string _mavenCommand;

        public JavaFixture() {
            _mavenCommand = SetUpMaven3();
        }

        /// <summary>
        ///   対応する言語のソースコードの拡張子を取得します．
        /// </summary>
        public override string Extension {
            get { return ".java"; }
        }

        /// <summary>
        ///   対応する言語のソースコードの拡張子を取得します．
        /// </summary>
        public override string CompiledExtension {
            get { return ".class"; }
        }

        /// <summary>
        ///   対応する言語のモデル生成器を取得します．
        /// </summary>
        public override UnifiedProgramGenerator ProgramGenerator {
            get { return JavaFactory.ProgramGenerator; }
        }

        /// <summary>
        ///   対応する言語のコード生成器を取得します．
        /// </summary>
        public override UnifiedCodeGenerator CodeGenerator {
            get { return JavaFactory.CodeGenerator; }
        }

        private static string DecorateToCompile(string statement) {
            return "class A { public void M1() {\n" + statement + "\n} }";
        }

        /// <summary>
        ///   指定したファイルのソースコードをデフォルトの設定でコンパイルします．
        /// </summary>
        /// <param name="workPath"> コンパイル時の作業ディレクトリのパス </param>
        /// <param name="srcPath"> コンパイル対象のソースコードのパス </param>
        public override void Compile(string workPath, string srcPath) {
            var args = new[] {
                    "\"" + srcPath + "\""
            };
            var arguments = string.Join(" ", args);
            CompileWithArguments(workPath, CompileCommand, arguments);
        }

        /// <summary>
        ///   コンパイル済みのコードのバイト列を取得します．
        /// </summary>
        /// <param name="path"> コンパイル済みのコードのパス </param>
        /// <returns> コンパイル済みのコードのバイト列 </returns>
        public override object GetCompiledByteCode(string path) {
            var args = new[] { "-c", path };
            var info = new ProcessStartInfo {
                    FileName = DisassembleCommand,
                    Arguments = string.Join(" ", args),
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
            };

            try {
                using (var p = Process.Start(info)) {
                    var str = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    if (p.ExitCode != 0) {
                        throw new InvalidOperationException(
                                "Failed to disassemble the exe file.");
                    }
                    return str;
                }
            } catch (Win32Exception e) {
                throw new InvalidOperationException(
                        "Failed to launch 'ildasmPath': " + DisassembleCommand,
                        e);
            }
        }

        private string SetUpMaven3() {
            var path = FixtureUtil.GetDownloadPath(LanguageName, "Maven3");
            var exePath = Path.Combine(
                    path, "apache-maven-3.0.3", "bin", "mvn.bat");
            if (Directory.Exists(path)
                &&
                Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                        .Any()) {
                return exePath;
            }
            Directory.CreateDirectory(path);
            DownloadAndUntgz(
                    "http://www.meisei-u.ac.jp/mirror/apache/dist//maven/binaries/apache-maven-3.0.3-bin.tar.gz",
                    path);
            return exePath;
        }

        private IEnumerable<TestCaseData> SetUpJUnit() {
            const string srcDirName = "src";
            return SetUpTestCaseData(
                    "junit4.8.2",
                    path => {
                        DownloadAndUnzip(
                                "https://github.com/downloads/KentBeck/junit/junit4.8.2.zip",
                                path);
                        var srcDirPath = Path.Combine(path, srcDirName);
                        var arcPath = Path.Combine(
                                path, "junit4.8.2", "junit-4.8.2-src.jar");
                        Extractor.Unzip(arcPath, srcDirPath);
                    },
                    (workPath, inPath) => {
                        workPath = Path.Combine(workPath, srcDirName);
                        var depPath = Path.Combine(
                                inPath, "junit4.8.2", "temp.hamcrest.source");
                        foreach (var srcPath in GetAllSourceFilePaths(workPath)) {
                            var args = new[] {
                                    "-cp",
                                    ".;\"" + depPath + "\"",
                                    "\"" + srcPath + "\"",
                            };
                            CompileWithArguments(
                                    workPath, CompileCommand,
                                    string.Join(" ", args));
                        }
                    });
        }

        private void CompileMaven(string workPath) {
            var pomPath =
                    Directory.EnumerateFiles(
                            workPath, "pom.xml", SearchOption.AllDirectories).
                            First();
            workPath = Path.GetDirectoryName(pomPath);
            CompileWithArguments(workPath, _mavenCommand, MavenArg);
        }

        private IEnumerable<TestCaseData> SetUpJdk() {
            return SetUpTestCaseData(
                    "jdk", path => {
                        var jdkPath = Directory.GetDirectories(
                                @"C:\Program Files\Java\")
                                .LastOrDefault(
                                        p =>
                                        Path.GetFileName(p).StartsWith("jdk1.6"));
                        if (jdkPath == null) {
                            return false;
                        }
                        var arcPath = Path.Combine(jdkPath, "src.zip");
                        Extractor.Unzip(arcPath, path);
                        return true;
                    });
        }

        private IEnumerable<TestCaseData> SetUpCraftBukkit() {
            return SetUpTestCaseData(
                    "CraftBukkit",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/Bukkit/CraftBukkit/zipball/master",
                            path),
                    CompileMaven);
        }

        private IEnumerable<TestCaseData> SetUpBukkit() {
            return SetUpTestCaseData(
                    "Bukkit",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/Bukkit/Bukkit/zipball/master",
                            path),
                    CompileMaven);
        }

        private IEnumerable<TestCaseData> SetUpJenkins() {
            return SetUpTestCaseData(
                    "jenkins-1.418",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/jenkinsci/jenkins/zipball/jenkins-1.418",
                            path));
        }

        private IEnumerable<TestCaseData> SetUpGameOfLife() {
            return SetUpTestCaseData(
                    "game-of-life_release-candidate-44",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/wakaleo/game-of-life/zipball/release-candidate-44",
                            path));
        }

        private IEnumerable<TestCaseData> SetUpJedis() {
            return SetUpTestCaseData(
                    "jedis-2.0.0",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/xetorthio/jedis/zipball/jedis-2.0.0",
                            path));
        }

        private IEnumerable<TestCaseData> SetUpZoie() {
            return SetUpTestCaseData(
                    "zoie-3.0.0",
                    path =>
                    DownloadAndUnzip(
                            "https://github.com/javasoze/zoie/zipball/release-3.0.0",
                            path));
        }
    }
}