#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif

#if !DISABLESTEAMWORKS
using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamApi.PlayerServices
{
    /// <summary>
    /// Manages the Steam Remote Storage system.
    /// </summary>
    public class SteamworksRemoteStorage : MonoBehaviour
    {
        private static SteamworksRemoteStorage s_instance;
        /// <summary>
        /// Provides singleton like access to the <see cref="SteamworksRemoteStorage"/> system
        /// </summary>
        public static SteamworksRemoteStorage Instance
        {
            get
            {
                if (s_instance == null)
                {
                    return new GameObject("HeathenSteamCloud").AddComponent<SteamworksRemoteStorage>();
                }
                else
                {
                    return s_instance;
                }
            }
        }
        /// <summary>
        /// Pointers to all available <see cref="SteamDataLibrary"/> objects that have been registered to the system. These represent the structure of unique types of save files.
        /// </summary>
        [Header("Remote Storage")]
        public List<SteamDataLibrary> GameDataModel = new List<SteamDataLibrary>();
        /// <summary>
        /// Pointers to all available data files located on the Steam Remote Storage system.
        /// </summary>
        public List<SteamDataFileAddress> SteamDataFilesIndex = new List<SteamDataFileAddress>();
        /// <summary>
        /// Occures when a file read operation is compelted.
        /// </summary>
        [Header("Events")]
        public UnityEvent FileReadAsyncComplete;
        /// <summary>
        /// Occures when a file write operation is completed.
        /// </summary>
        public UnityEvent FileWriteAsyncComplete;
        /// <summary>
        /// For internal use
        /// </summary>
        public Callback<RemoteStorageFileReadAsyncComplete_t> fileReadAsyncComplete;
        /// <summary>
        /// For internal use
        /// </summary>
        public Callback<RemoteStorageFileShareResult_t> fileShareResult;
        /// <summary>
        /// For internal use
        /// </summary>
        public Callback<RemoteStorageFileWriteAsyncComplete_t> fileWriteAsyncComplete;

        private List<SteamDataFile> pendingDataRequests = new List<SteamDataFile>();

        /// <summary>
        /// Checks if the account wide Steam Cloud setting is enabled for this user; or if they disabled it in the Settings->Cloud dialog.
        /// Ensure that you are also checking IsCloudEnabledForApp, as these two options are mutually exclusive.
        /// </summary>
        public bool IsCloudEnabledForAccount
        {
            get
            {
                return SteamRemoteStorage.IsCloudEnabledForAccount();
            }
        }

        /// <summary>
        /// Checks if the per game Steam Cloud setting is enabled for this user; or if they disabled it in the Game Properties->Update dialog.
        /// Ensure that you are also checking IsCloudEnabledForAccount, as these two options are mutually exclusive.
        /// It's generally recommended that you allow the user to toggle this setting within your in-game options, you can toggle it with SetCloudEnabledForApp.
        /// </summary>
        public bool IsCloudEnabledForApp
        {
            get
            {
                return SteamRemoteStorage.IsCloudEnabledForApp();
            }
        }

        void Start()
        {
            s_instance = this;

            fileReadAsyncComplete = Callback<RemoteStorageFileReadAsyncComplete_t>.Create(HandleFileReadAsyncComplete);
            fileShareResult = Callback<RemoteStorageFileShareResult_t>.Create(HandleFileShareResult);
            fileWriteAsyncComplete = Callback<RemoteStorageFileWriteAsyncComplete_t>.Create(HandleFileWriteAsyncComplete);
        }

        #region Event Handlers
        private void HandleFileWriteAsyncComplete(RemoteStorageFileWriteAsyncComplete_t param)
        {
            //TODO: Coming Soon!
            FileWriteAsyncComplete.Invoke();
        }

        private void HandleFileShareResult(RemoteStorageFileShareResult_t param)
        {
            //TODO: Coming Soon!
        }

        private void HandleFileReadAsyncComplete(RemoteStorageFileReadAsyncComplete_t param)
        {
            //See if we have a pending data request that matches this
            if (pendingDataRequests.Exists(p => p.apiCall.Value == param.m_hFileReadAsync))
            {
                //We do so fetch it and load the data
                var data = pendingDataRequests.First(p => p.apiCall.Value == param.m_hFileReadAsync);
                pendingDataRequests.Remove(data);
                data.result = param.m_eResult;
                //If the request result was okay fetch the binary data
                if (data.result == EResult.k_EResultOK)
                {
                    data.binaryData = new byte[data.address.fileSize];
                    if(!SteamRemoteStorage.FileReadAsyncComplete(param.m_hFileReadAsync, data.binaryData, (uint)data.binaryData.Length))
                    {
                        //If we failed to read the binary data update the result to fail
                        data.result = EResult.k_EResultFail;
                    }
                    else
                    {
                        //Data succesfuly loaded test for library
                        var lib = GetDataModelLibrary(data.address.fileName);
                        if(lib != null)
                        {
                            lib.activeFile = data;
                            data.WriteToLibrary(lib);
                        }
                    }
                }
            }

            FileReadAsyncComplete.Invoke();
        }
        #endregion

        /// <summary>
        /// Populates the SteamDataFilesIndex with all files available to this Steam User
        /// </summary>
        public void RefreshDataFilesIndex()
        {
            SteamDataFilesIndex.Clear();
            ClearAvailableLibraries();
            var count = SteamRemoteStorage.GetFileCount();
            for (int i = 0; i < count; i++)
            {
                var size = 0;
                var name = SteamRemoteStorage.GetFileNameAndSize(i, out size);
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(name);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(timeStamp);
                var data = new SteamDataFileAddress()
                {
                    fileIndex = i,
                    fileName = name,
                    fileSize = size,
                    UtcTimestamp = dateTime
                };
                SteamDataFilesIndex.Add(data);

                var lib = GetDataModelLibrary(name);
                if(lib != null)
                {
                    lib.availableFiles.Add(data);
                }
            }
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                if (GameDataModel.Exists(p => fileName.StartsWith(p.filePrefix)))
                {
                    return GameDataModel.First(p => fileName.StartsWith(p.filePrefix));
                }
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(SteamDataFileAddress address)
        {
            return GetDataModelLibrary(address.fileName);
        }

        /// <summary>
        /// Gets the data model type library that this file should belong to
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public SteamDataLibrary GetDataModelLibrary(SteamDataFile file)
        {
            if (file != null)
            {
                return GetDataModelLibrary(file.address.fileName);
            }
            else
                return null;
        }

        private void ClearAvailableLibraries()
        {
            foreach(var lib in GameDataModel)
            {
                lib.availableFiles.Clear();
            }
        }
        
        // Wrappers around standard Steamworks funcitonality for ease of access and integraiton with Heathen Systems
        #region Steamworks Native
        /// <summary>
        /// Toggles whether the Steam Cloud is enabled for your application.
        /// This setting can be queried with IsCloudEnabledForApp.
        /// </summary>
        /// <remarks>
        /// This must only ever be called as the direct result of the user explicitly requesting that it's enabled or not. This is typically accomplished with a checkbox within your in-game options.
        /// </remarks>
        /// <param name="enable"></param>
        public void SetCloudEnabledForApp(bool enable)
        {
            SteamRemoteStorage.SetCloudEnabledForApp(enable);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(SteamDataFile file, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(file.address.fileName, platform);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(SteamDataFileAddress address, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(address.fileName, platform);
        }

        /// <summary>
        /// Allows you to specify which operating systems a file will be synchronized to.
        /// Use this if you have a multiplatform game but have data which is incompatible between platforms.
        /// Files default to k_ERemoteStoragePlatformAll when they are first created.You can use the bitwise OR operator, "|" to specify multiple platforms.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public bool SetSyncPlatforms(string fileName, ERemoteStoragePlatform platform)
        {
            return SteamRemoteStorage.SetSyncPlatforms(fileName, platform);
        }

        /// <summary>
        /// Returns the UTC timestamp from the Steam Remote Storage system
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>UTC time stamp</returns>
        public DateTime GetFileTimestamp(string fileName)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (SteamRemoteStorage.FileExists(fileName))
            {
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(name);
                dateTime.AddSeconds(timeStamp);
            }
            return dateTime;
        }

        /// <summary>
        /// Returns the UTC timestamp from the Steam Remote Storage system
        /// </summary>
        /// <param name="address"></param>
        /// <returns>UTC time stamp</returns>
        public DateTime GetFileTimestamp(SteamDataFileAddress address)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (SteamRemoteStorage.FileExists(address.fileName))
            {
                var timeStamp = SteamRemoteStorage.GetFileTimestamp(name);
                dateTime = dateTime.AddSeconds(timeStamp);
                address.UtcTimestamp = dateTime;
            }
            return dateTime;
        }

        /// <summary>
        /// Deletes a file from the local disk, and propagates that delete to the cloud.
        /// This is meant to be used when a user actively deletes a file.Use FileForget if you want to remove a file from the Steam Cloud but retain it on the users local disk.
        /// When a file has been deleted it can be re-written with FileWrite to reupload it to the Steam Cloud
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists and has been successfully deleted; otherwise, false if the file did not exist.</returns>
        public bool FileDelete(string fileName)
        {
            if (SteamDataFilesIndex.Exists(p => p.fileName == fileName))
            {
                var address = SteamDataFilesIndex.First(p => p.fileName == fileName);

                SteamDataFilesIndex.Remove(address);
                var lib = GetDataModelLibrary(address.fileName);
                if (lib != null)
                    lib.availableFiles.Remove(address);
            }

            return SteamRemoteStorage.FileDelete(fileName);
        }

        /// <summary>
        /// Deletes a file from the local disk, and propagates that delete to the cloud.
        /// This is meant to be used when a user actively deletes a file.Use FileForget if you want to remove a file from the Steam Cloud but retain it on the users local disk.
        /// When a file has been deleted it can be re-written with FileWrite to reupload it to the Steam Cloud
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true if the file exists and has been successfully deleted; otherwise, false if the file did not exist.</returns>
        public bool FileDelete(SteamDataFileAddress address)
        {
            SteamDataFilesIndex.Remove(address);
            var lib = GetDataModelLibrary(address.fileName);
            if (lib != null)
                lib.availableFiles.Remove(address);

            return SteamRemoteStorage.FileDelete(address.fileName);
        }

        /// <summary>
        /// Checks whether the specified file exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists; otherwise, false</returns>
        public bool FileExists(string fileName)
        {
            return SteamRemoteStorage.FileExists(fileName);
        }

        /// <summary>
        /// Checks whether the specified file exists.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>true if the file exists; otherwise, false</returns>
        public bool FileExists(SteamDataFileAddress address)
        {
            return SteamRemoteStorage.FileExists(address.fileName);
        }

        /// <summary>
        /// Starts an asynchronous read from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A data file containing the binary result of the read</returns>
        public SteamDataFile FileRead(string fileName)
        {
            var address = new SteamDataFileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                address = SteamDataFilesIndex.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            var data = new SteamDataFile()
            {
                address = address,
                binaryData = buffer,
                apiCall = null,
                result = EResult.k_EResultOK
            };

            //Test for library link
            var lib = GetDataModelLibrary(address.fileName);
            if (lib != null)
            {
                lib.activeFile = data;
                data.WriteToLibrary(lib);
            }

            return data;
        }

        public byte[] FileReadData(string fileName)
        {
            var size = SteamRemoteStorage.GetFileSize(fileName);

            var buffer = new byte[size];
            SteamRemoteStorage.FileRead(fileName, buffer, size);
            
            return buffer;
        }

        /// <summary>
        /// Starts an asynchronous read from a file.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>A data file containing the binary result of the read</returns>
        public SteamDataFile FileRead(SteamDataFileAddress address)
        {
            var buffer = new byte[address.fileSize];
            SteamRemoteStorage.FileRead(address.fileName, buffer, buffer.Length);
            var data = new SteamDataFile()
            {
                address = address,
                binaryData = buffer,
                apiCall = null,
                result = EResult.k_EResultOK
            };

            //Test for library link
            var lib = GetDataModelLibrary(address.fileName);
            if(lib != null)
            {
                lib.activeFile = data;
                data.WriteToLibrary(lib);
            }

            return data;
        }

        /// <summary>
        /// Starts an asynchronous read from a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>A data file pointer that will update when the file read has been completed</returns>
        public SteamDataFile FileReadAsync(string fileName)
        {
            var address = new SteamDataFileAddress()
            {
                fileIndex = -1,
                fileName = fileName
            };

            if (SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                address = SteamDataFilesIndex.First(p => p.fileName == fileName);
            else
            {
                GetFileTimestamp(address);
            }

            var data = new SteamDataFile()
            {
                address = address
            };
            data.apiCall = SteamRemoteStorage.FileReadAsync(address.fileName, 0, (uint)address.fileSize);
            pendingDataRequests.Add(data);
            return data;
        }

        /// <summary>
        /// Starts an asynchronous read from a file.
        /// </summary>
        /// <param name="address"></param>
        /// <returns>A data file pointer that will update when the file read has been completed</returns>
        public SteamDataFile FileReadAsync(SteamDataFileAddress address)
        {
            var data = new SteamDataFile()
            {
                address = address
            };
            data.apiCall = SteamRemoteStorage.FileReadAsync(address.fileName, 0, (uint)address.fileSize);
            pendingDataRequests.Add(data);
            return data;
        }

        /// <summary>
        /// Deletes the file from remote storage, but leaves it on the local disk and remains accessible from the API.
        /// </summary>
        /// <remarks>
        /// When you are out of Cloud space, this can be used to allow calls to FileWrite to keep working without needing to make the user delete files.
        /// How you decide which files to forget are up to you.It could be a simple Least Recently Used(LRU) queue or something more complicated.
        /// Requiring the user to manage their Cloud-ized files for a game, while is possible to do, it is never recommended.For instance, "Which file would you like to delete so that you may store this new one?" removes a significant advantage of using the Cloud in the first place: its transparency.
        /// Once a file has been deleted or forgotten, calling FileWrite will resynchronize it in the Cloud. Rewriting a forgotten file is the only way to make it persisted again.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <returns>true if the file exists and has been successfully forgotten; otherwise, false.</returns>
        public bool FileForget(string fileName)
        {
            return SteamRemoteStorage.FileForget(fileName);
        }

        /// <summary>
        /// Deletes the file from remote storage, but leaves it on the local disk and remains accessible from the API.
        /// </summary>
        /// <remarks>
        /// When you are out of Cloud space, this can be used to allow calls to FileWrite to keep working without needing to make the user delete files.
        /// How you decide which files to forget are up to you.It could be a simple Least Recently Used(LRU) queue or something more complicated.
        /// Requiring the user to manage their Cloud-ized files for a game, while is possible to do, it is never recommended.For instance, "Which file would you like to delete so that you may store this new one?" removes a significant advantage of using the Cloud in the first place: its transparency.
        /// Once a file has been deleted or forgotten, calling FileWrite will resynchronize it in the Cloud. Rewriting a forgotten file is the only way to make it persisted again.
        /// </remarks>
        /// <param name="address"></param>
        /// <returns>true if the file exists and has been successfully forgotten; otherwise, false.</returns>
        public bool FileForget(SteamDataFileAddress address)
        {
            return SteamRemoteStorage.FileForget(address.fileName);
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="file"></param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public bool FileWrite(SteamDataFile file)
        {
            if (file != null && file.binaryData.Length > 0 && !string.IsNullOrEmpty(file.address.fileName))
            {
                //Test for linked library and refresh as required
                if (file.linkedLibrary != null)
                    file.ReadFromLibrary(file.linkedLibrary);

                if (SteamRemoteStorage.FileWrite(file.address.fileName, file.binaryData, file.binaryData.Length))
                {
                    file.address.UtcTimestamp = GetFileTimestamp(file.address);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                //Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (file.binaryData.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(file.address.fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>true if the write was successful. Otherwise, false.
        /// </returns>
        public bool FileWrite(string fileName, byte[] data)
        {
            if (data.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                //See if file has an existing indexed address, if not create one
                if(!SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                {
                    var address = new SteamDataFileAddress()
                    {
                        fileIndex = -1,
                        fileName = fileName,
                        fileSize = data.Length,
                        UtcTimestamp = DateTime.UtcNow
                    };

                    SteamDataFilesIndex.Add(address);
                }

                var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                if(!result)
                {
                    SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                    if(available < Convert.ToUInt64(data.Length))
                    {
                        Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                    }
                }
                else
                {
                    Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                }

                return result;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (data.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Writes file to the target file from a byte[] of raw data.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool FileWriteData(string fileName, byte[] data)
        {
            if (data.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                //See if file has an existing indexed address, if not create one
                if (!SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                {
                    var address = new SteamDataFileAddress()
                    {
                        fileIndex = -1,
                        fileName = fileName,
                        fileSize = data.Length,
                        UtcTimestamp = DateTime.UtcNow
                    };

                    SteamDataFilesIndex.Add(address);
                }

                var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                if (!result)
                {
                    SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                    if (available < Convert.ToUInt64(data.Length))
                    {
                        Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                    }
                }
                else
                {
                    Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                }

                return result;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (data.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file, writes the bytes to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// May return false under the following conditions:
        /// The file you're trying to write is larger than 100MiB as defined by k_unMaxCloudFileChunkSize.
        /// cubData is less than 0.
        /// pvData is NULL.
        /// You tried to write to an invalid path or filename.Because Steam Cloud is cross platform the files need to have valid names on all supported OSes and file systems. See Microsoft's documentation on Naming Files, Paths, and Namespaces.
        /// The current user's Steam Cloud storage quota has been exceeded. They may have run out of space, or have too many files.
        /// Steam could not write to the disk, the location might be read-only.
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="lib">The data library containing the data to be saved, this will be matched to a Game Data Model entry and if found a Save Data File will be generated and linked</param>
        /// <returns>true if the write was successful. Otherwise, false. 
        /// See https://partner.steamgames.com/doc/api/ISteamRemoteStorage#FileWrite for more information
        /// </returns>
        public bool FileWrite(string fileName, SteamDataLibrary lib)
        {
            if (lib != null && !string.IsNullOrEmpty(fileName))
            {
                //Test for a data model link
                if (GameDataModel.Exists(p => p == lib))
                {
                    var address = new SteamDataFileAddress();

                    //Test for an existing address with this name
                    if (SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                        address = SteamDataFilesIndex.First(p => p.fileName == fileName);
                    else
                    {
                        address.fileIndex = -1;
                        address.fileName = fileName;
                        address.UtcTimestamp = DateTime.UtcNow;
                        SteamDataFilesIndex.Add(address);
                        lib.availableFiles.Add(address);
                    }

                    SteamDataFile file = new SteamDataFile()
                    {
                        address = address,
                        linkedLibrary = lib,
                        result = EResult.k_EResultOK
                    };

                    file.ReadFromLibrary(file.linkedLibrary);

                    lib.activeFile = file;

                    lib.SyncToBuffer(out file.binaryData);

                    var result = SteamRemoteStorage.FileWrite(fileName, file.binaryData, file.binaryData.Length);
                    if (!result)
                    {
                        SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                        if (available < Convert.ToUInt64(file.binaryData.Length))
                        {
                            Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                        }
                    }
                    else
                    {
                        Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                    }

                    return result;
                }
                else
                {
                    //Not linked to a model so just save it and move on
                    byte[] data;
                    lib.SyncToBuffer(out data);
                    var result = SteamRemoteStorage.FileWrite(fileName, data, data.Length);
                    if (!result)
                    {
                        SteamRemoteStorage.GetQuota(out ulong total, out ulong available);
                        if (available < Convert.ToUInt64(data.Length))
                        {
                            Debug.LogWarning("Insufficent storage space available on the Steam Remote Storage target.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... Please consult your Steamworks documentaiton regarding Steam Remote Storage.");
                        }
                    }
                    else
                    {
                        Debug.Log("File " + fileName + " saved to Steam Remote Storage.");
                    }

                    return result;
                }
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (lib == null ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="file"></param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public bool FileWriteAsync(SteamDataFile file)
        {
            if (file != null && file.binaryData.Length > 0 && !string.IsNullOrEmpty(file.address.fileName))
            {
                //Test for linked library and refresh as required
                if (file.linkedLibrary != null)
                    file.ReadFromLibrary(file.linkedLibrary);

                SteamRemoteStorage.FileWriteAsync(file.address.fileName, file.binaryData, (uint)file.binaryData.Length);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (file.binaryData.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(file.address.fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public bool FileWriteAsync(string fileName, byte[] data)
        {
            if (data.Length > 0 && !string.IsNullOrEmpty(fileName))
            {
                SteamRemoteStorage.FileWriteAsync(fileName, data, (uint)data.Length);
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (data.Length < 0 ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }

        /// <summary>
        /// Creates a new file and asynchronously writes the raw byte data to the Steam Cloud, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <remarks>
        /// Note that this will trigger a RemoteStorageFileWriteAsyncComplete_t callback indicating the result state however there is no good way to link this with a specific file write request
        /// </remarks>
        /// <param name="fileName"></param>
        /// <param name="library">The data library containing the data to be saved, this will be matched to a Game Data Model entry and if found a Save Data File will be generated and linked</param>
        /// <returns>true if the call was issued succesfuly, false otherwise</returns>
        public bool FileWriteAsync(string fileName, SteamDataLibrary lib)
        {
            if (lib != null && !string.IsNullOrEmpty(fileName))
            {
                    var address = new SteamDataFileAddress();

                    //Test for an existing address with this name
                    if (SteamDataFilesIndex.Exists(p => p.fileName == fileName))
                        address = SteamDataFilesIndex.First(p => p.fileName == fileName);
                    else
                    {
                        address.fileIndex = -1;
                        address.fileName = fileName;
                        address.UtcTimestamp = DateTime.UtcNow;
                        SteamDataFilesIndex.Add(address);
                        lib.availableFiles.Add(address);
                    }

                    SteamDataFile file = new SteamDataFile()
                    {
                        address = address,
                        linkedLibrary = lib,
                        result = EResult.k_EResultOK
                    };

                    lib.activeFile = file;

                    lib.SyncToBuffer(out file.binaryData);
                    SteamRemoteStorage.FileWriteAsync(fileName, file.binaryData, (uint)file.binaryData.Length);
                    return true;
            }
            else
            {
                Debug.LogWarning("Failed to save the file to the Steam Remote Storage ... " + (lib == null ? "You did not pass any data to be saved! " : "") + (string.IsNullOrEmpty(fileName) ? "You did not provide a valid file name! " : ""));
                return false;
            }
        }
        #endregion
    }
}
#endif
