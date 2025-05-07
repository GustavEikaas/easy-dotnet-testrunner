# Easy Dotnet Testrunner Server

## Description

**Easy Dotnet Testrunner Server** is the lightweight C# JSON-RPC server powering the testrunner in [easy-dotnet.nvim](https://github.com/GustavEikaas/easy-dotnet.nvim) Neovim plugin.

It powers the plugin’s test discovery and execution features, supporting both **VSTest** and **Microsoft.TestPlatform (MTP)**. The server communicates via named pipes using JSON-RPC and provides a unified response format for the plugin.

## Features

* JSON-RPC 2.0 communication over named pipes
* Supports both VSTest and MTP-based test frameworks
* Unified test results and structure format
* Asynchronous, multi-client server support

## Use Case

This server is an internal component of the `easy-dotnet.nvim` plugin and is **not intended for standalone use**.

