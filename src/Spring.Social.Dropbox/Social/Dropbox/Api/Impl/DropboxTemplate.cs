﻿#region License

/*
 * Copyright 2002-2012 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System;
using System.Text;
using System.Collections.Generic;
#if SILVERLIGHT
using Spring.Collections.Specialized;
#else
using System.Collections.Specialized;
#endif
#if NET_4_0 || SILVERLIGHT_5
using System.Threading;
using System.Threading.Tasks;
#endif

using Spring.IO;
using Spring.Json;
using Spring.Rest.Client;
using Spring.Social.OAuth1;
using Spring.Http;
using Spring.Http.Converters;
using Spring.Http.Converters.Json;

using Spring.Social.Dropbox.Api.Impl.Json;

namespace Spring.Social.Dropbox.Api.Impl
{
    /// <summary>
    /// This is the central class for interacting with Dropbox.
    /// </summary>
    /// <remarks>
    /// All Dropbox operations require OAuth authentication.
    /// </remarks>
    /// <author>Bruno Baia</author>
    public class DropboxTemplate : AbstractOAuth1ApiBinding, IDropbox 
    {
        private static readonly Uri API_URI_BASE = new Uri("https://api.dropbox.com/1/");

        private AccessLevel accessLevel;
        private string root;

        /// <summary>
        /// Creates a new instance of <see cref="DropboxTemplate"/>.
        /// </summary>
        /// <param name="consumerKey">The application's API key.</param>
        /// <param name="consumerSecret">The application's API secret.</param>
        /// <param name="accessToken">An access token acquired through OAuth authentication with Dropbox.</param>
        /// <param name="accessTokenSecret">An access token secret acquired through OAuth authentication with Dropbox.</param>
        /// <param name="accessLevel">The application access level.</param>
        public DropboxTemplate(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, AccessLevel accessLevel) 
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret)
        {
            this.accessLevel = accessLevel;
            this.root = this.accessLevel == Api.AccessLevel.AppFolder ? "sandbox" : "dropbox";
	    }

        #region IDropbox Members

        /// <summary>
        /// Gets the application access level. 
        /// </summary>
        public AccessLevel AccessLevel
        {
            get { return accessLevel; }
        }

#if NET_4_0 || SILVERLIGHT_5
        /// <summary>
        /// Asynchronously retrieves the authenticated user's Dropbox profile details.
        /// </summary>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a <see cref="DropboxProfile"/> object representing the user's profile.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<DropboxProfile> GetUserProfileAsync()
        {
            NameValueCollection parameters = new NameValueCollection();
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObjectAsync<DropboxProfile>(this.BuildUrl("account/info", parameters));
        }

        /// <summary>
        /// Asynchronously creates a folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the new folder to create relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the new folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> CreateFolderAsync(string path)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/create_folder", request);
        }

        /// <summary>
        /// Asynchronously deletes a file or folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder to be deleted relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the deleted file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> DeleteAsync(string path)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/delete", request);
        }

        /// <summary>
        /// Asynchronously moves a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the moved file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> MoveAsync(string fromPath, string toPath)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/move", request);
        }

        /// <summary>
        /// Asynchronously copies a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the moved file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> CopyAsync(string fromPath, string toPath)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/copy", request);
        }

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the task.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the uploaded file.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> UploadFileAsync(IResource file, string path, CancellationToken cancellationToken)
        {
            return this.UploadFileAsync(file, path, true, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <param name="overwrite">
        /// If <see langword="true"/>, the existing file will be overwritten by the new one. 
        /// If <see langword="false"/>, the new file will be automatically renamed. 
        /// The new name can be obtained from the returned metadata.
        /// </param>
        /// <param name="revision">
        /// The revision of the file you're editing or null if this is a new upload. 
        /// If <paramref name="revision"/> matches the latest version of the file on the user's Dropbox, that file will be replaced.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the task.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the uploaded file.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> UploadFileAsync(IResource file, string path, bool overwrite, string revision, CancellationToken cancellationToken)
        {
            return this.RestTemplate.ExchangeAsync<Entry>(
                this.BuildUploadUrl(path, overwrite, revision), HttpMethod.PUT, new HttpEntity(file), cancellationToken)
                .ContinueWith<Entry>(task =>
                {
                    return task.Result.Body;
                });
        }

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the task.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// an array of bytes containing the file's content.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<byte[]> DownloadFileAsync(string path, CancellationToken cancellationToken)
        {
            return this.DownloadFileAsync(path, null, cancellationToken);
        }

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <param name="revision">The revision of the file to retrieve.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that will be assigned to the task.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// an array of bytes containing the file's content.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<byte[]> DownloadFileAsync(string path, string revision, CancellationToken cancellationToken)
        {
            return this.RestTemplate.ExchangeAsync<byte[]>(
                this.BuildDownloadUrl(path, revision), HttpMethod.GET, null, cancellationToken)
                .ContinueWith<byte[]>(task =>
                {
                    return task.Result.Body;
                });
        }

        /// <summary>
        /// Asynchronously retrieves file or folder metadata.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> GetMetadataAsync(string path)
        {
            return this.GetMetadataAsync(path, new MetadataParameters());
        }

        /// <summary>
        /// Asynchronously retrieves file or folder metadata. 
        /// May return <see langword="null"/> if an hash is provided through parameters.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <param name="parameters">The parameters for retrieving file and folder metadata.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> GetMetadataAsync(string path, MetadataParameters parameters)
        {
            return this.RestTemplate.GetForObjectAsync<Entry>(this.BuildMetadataUrl(path, parameters));
        }

        /// <summary>
        /// Asynchronously obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a list of metadata <see cref="Entry">entries</see>.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<IList<Entry>> GetRevisionsAsync(string path)
        {
            return this.GetRevisionsAsync(path, 0);
        }

        /// <summary>
        /// Asynchronously obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revLimit">
        /// The maximum numbers of revisions to retrieve. Default is 10, Max is 1,000.
        /// </param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a list of metadata <see cref="Entry">entries</see>.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<IList<Entry>> GetRevisionsAsync(string path, int revLimit)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (revLimit > 0)
            {
                parameters.Add("rev_limit", revLimit.ToString());
            }
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObjectAsync<IList<Entry>>(this.BuildUrl("revisions/", path, parameters));
        }

        /// <summary>
        /// Asynchronously restores a file path to a previous revision.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revision">The revision of the file to restore.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a metadata <see cref="Entry"/> for the restored file. 
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<Entry> RestoreAsync(string path, string revision)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("rev", revision);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>(this.BuildUrl("restore/", path), request);
        }

        /// <summary>
        /// Asynchronously searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<IList<Entry>> SearchAsync(string path, string query)
        {
            return this.SearchAsync(path, query, 0, false);
        }

        /// <summary>
        /// Asynchronously searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <param name="fileLimit">
        /// The maximum numbers of file entries to retrieve. The maximum and default value is 1000.
        /// </param>
        /// <param name="includeDeleted">
        /// If <see langword="true"/>, then files and folders that have been deleted will also be included in the search.
        /// </param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<IList<Entry>> SearchAsync(string path, string query, int fileLimit, bool includeDeleted)
        {
            NameValueCollection request = this.BuildSearchParameters(path, query, fileLimit, includeDeleted);
            return this.RestTemplate.PostForObjectAsync<IList<Entry>>(this.BuildUrl("search/", path), request);
        }

        /// <summary>
        /// Asynchronously creates and returns a shareable link to files or folders.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the file or folder you want a sharable link to, relative to root.
        /// </param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a shareable link to the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<DropboxLink> GetShareableLinkAsync(string path)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<DropboxLink>(this.BuildUrl("shares/", path), request);
        }

        /// <summary>
        /// Asynchronously returns a link for streaming media files.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the media file you want a direct link to, relative to root.
        /// </param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// a direct link to the media file.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<DropboxLink> GetMediaLinkAsync(string path)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<DropboxLink>(this.BuildUrl("media/", path), request);
        }

        /// <summary>
        /// Asynchronously downloads a thumbnail for an image.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the image file you want to thumbnail, relative to root.
        /// </param>
        /// <param name="format">The image format of the thumbnail to download.</param>
        /// <param name="size">The size of the thumbnail to download.</param>
        /// <returns>
        /// A <code>Task</code> that represents the asynchronous operation that can return 
        /// an array of bytes containing the thumbnail's content.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Task<byte[]> DownloadThumbnailAsync(string path, ThumbnailFormat format, ThumbnailSize size)
        {
            return this.RestTemplate.GetForObjectAsync<byte[]>(this.BuildThumbnailsUrl(path, format, size));
        }
#else
#if !SILVERLIGHT
        /// <summary>
        /// Retrieves the authenticated user's Dropbox profile details.
        /// </summary>
        /// <returns>A <see cref="DropboxProfile"/> object representing the user's profile.</returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public DropboxProfile GetUserProfile()
        {
            NameValueCollection parameters = new NameValueCollection();
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObject<DropboxProfile>(this.BuildUrl("account/info", parameters));
        }

        /// <summary>
        /// Creates a folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the new folder to create relative to root.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the new folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry CreateFolder(string path)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<Entry>("fileops/create_folder", request);
        }

        /// <summary>
        /// Deletes a file or folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder to be deleted relative to root.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the deleted file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry Delete(string path)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<Entry>("fileops/delete", request);
        }

        /// <summary>
        /// Moves a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the moved file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry Move(string fromPath, string toPath)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<Entry>("fileops/move", request);
        }

        /// <summary>
        /// Copies a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the moved file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry Copy(string fromPath, string toPath)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<Entry>("fileops/copy", request);
        }

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <returns>A metadata <see cref="Entry"/> for the uploaded file.</returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry UploadFile(IResource file, string path)
        {
            return this.UploadFile(file, path, true, null);
        }

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <param name="overwrite">
        /// If <see langword="true"/>, the existing file will be overwritten by the new one. 
        /// If <see langword="false"/>, the new file will be automatically renamed. 
        /// The new name can be obtained from the returned metadata.
        /// </param>
        /// <param name="revision">
        /// The revision of the file you're editing. 
        /// If <paramref name="revision"/> matches the latest version of the file on the user's Dropbox, that file will be replaced.
        /// </param>
        /// <returns>A metadata <see cref="Entry"/> for the uploaded file.</returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry UploadFile(IResource file, string path, bool overwrite, string revision)
        {
            return this.RestTemplate.Exchange<Entry>(
                this.BuildUploadUrl(path, overwrite, revision), HttpMethod.PUT, new HttpEntity(file)).Body;
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <returns>An array of bytes containing the file's content.</returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public byte[] DownloadFile(string path)
        {
            return this.DownloadFile(path, null);
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <param name="revision">The revision of the file to retrieve.</param>
        /// <returns>An array of bytes containing the file's content.</returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public byte[] DownloadFile(string path, string revision)
        {
            return this.RestTemplate.GetForObject<byte[]>(this.BuildDownloadUrl(path, revision));
        }

        /// <summary>
        /// Retrieves file or folder metadata.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry GetMetadata(string path)
        {
            return this.GetMetadata(path, new MetadataParameters());
        }

        /// <summary>
        /// Retrieves file or folder metadata. 
        /// May return <see langword="null"/> if an hash is provided through parameters.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <param name="parameters">The parameters for retrieving file and folder metadata.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry GetMetadata(string path, MetadataParameters parameters)
        {
            return this.RestTemplate.GetForObject<Entry>(this.BuildMetadataUrl(path, parameters));
        }

        /// <summary>
        /// Obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <returns>
        /// A list of metadata <see cref="Entry">entries</see>.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public IList<Entry> GetRevisions(string path)
        {
            return this.GetRevisions(path, 0);
        }

        /// <summary>
        /// Obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revLimit">
        /// The maximum numbers of revisions to retrieve. Default is 10, Max is 1,000.
        /// </param>
        /// <returns>
        /// A list of metadata <see cref="Entry">entries</see>.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public IList<Entry> GetRevisions(string path, int revLimit)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (revLimit > 0)
            {
                parameters.Add("rev_limit", revLimit.ToString());
            }
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObject<IList<Entry>>(this.BuildUrl("revisions/", path, parameters));
        }

        /// <summary>
        /// Restores a file path to a previous revision.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revision">The revision of the file to restore.</param>
        /// <returns>
        /// A metadata <see cref="Entry"/> for the restored file. 
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public Entry Restore(string path, string revision)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("rev", revision);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<Entry>(this.BuildUrl("restore/", path), request);
        }

        /// <summary>
        /// Searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <returns>
        /// A list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public IList<Entry> Search(string path, string query)
        {
            return this.Search(path, query, 0, false);
        }

        /// <summary>
        /// Searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <param name="fileLimit">
        /// The maximum numbers of file entries to retrieve. The maximum and default value is 1000.
        /// </param>
        /// <param name="includeDeleted">
        /// If <see langword="true"/>, then files and folders that have been deleted will also be included in the search.
        /// </param>
        /// <returns>
        /// A list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public IList<Entry> Search(string path, string query, int fileLimit, bool includeDeleted)
        {
            NameValueCollection request = this.BuildSearchParameters(path, query, fileLimit, includeDeleted);
            return this.RestTemplate.PostForObject<IList<Entry>>(this.BuildUrl("search/", path), request);
        }

        /// <summary>
        /// Creates and returns a shareable link to files or folders.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the file or folder you want a sharable link to, relative to root.
        /// </param>
        /// <returns>
        /// A shareable link to the file or folder.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public DropboxLink GetShareableLink(string path)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<DropboxLink>(this.BuildUrl("shares/", path), request);
        }

        /// <summary>
        /// Returns a link for streaming media files.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the media file you want a direct link to, relative to root.
        /// </param>
        /// <returns>
        /// A direct link to the media file.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public DropboxLink GetMediaLink(string path)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObject<DropboxLink>(this.BuildUrl("media/", path), request);
        }

        /// <summary>
        /// Downloads a thumbnail for an image.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the image file you want to thumbnail, relative to root.
        /// </param>
        /// <param name="format">The image format of the thumbnail to download.</param>
        /// <param name="size">The size of the thumbnail to download.</param>
        /// <returns>
        /// An array of bytes containing the thumbnail's content.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public byte[] DownloadThumbnail(string path, ThumbnailFormat format, ThumbnailSize size)
        {
            return this.RestTemplate.GetForObject<byte[]>(this.BuildThumbnailsUrl(path, format, size));
        }
#endif

        /// <summary>
        /// Asynchronously retrieves the authenticated user's Dropbox profile details.
        /// </summary>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a <see cref="DropboxProfile"/>object representing the user's profile.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetUserProfileAsync(Action<RestOperationCompletedEventArgs<DropboxProfile>> operationCompleted)
        {
            NameValueCollection parameters = new NameValueCollection();
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObjectAsync<DropboxProfile>(this.BuildUrl("account/info", parameters), operationCompleted);
        }

        /// <summary>
        /// Asynchronously creates a folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the new folder to create relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the new folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler CreateFolderAsync(string path, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/create_folder", request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously deletes a file or folder.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder to be deleted relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the deleted file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler DeleteAsync(string path, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("path", path);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/delete", request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously moves a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the moved file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler MoveAsync(string fromPath, string toPath, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/move", request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously copies a file or folder to a new location.
        /// </summary>
        /// <param name="fromPath">The path to the file or folder to be copied from, relative to root.</param>
        /// <param name="toPath">The destination path, including the new name for the file or folder, relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the moved file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler CopyAsync(string fromPath, string toPath, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("root", this.root);
            request.Add("from_path", fromPath);
            request.Add("to_path", toPath);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>("fileops/copy", request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the uploaded file.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler UploadFileAsync(IResource file, string path, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            return this.UploadFileAsync(file, path, true, null, operationCompleted);
        }

        /// <summary>
        /// Asynchronously uploads a file.
        /// </summary>
        /// <param name="file">The file resource to be uploaded.</param>
        /// <param name="path">
        /// The path to the file you want to write to, relative to root. 
        /// This parameter should not point to a folder.
        /// </param>
        /// <param name="overwrite">
        /// If <see langword="true"/>, the existing file will be overwritten by the new one. 
        /// If <see langword="false"/>, the new file will be automatically renamed. 
        /// The new name can be obtained from the returned metadata.
        /// </param>
        /// <param name="revision">
        /// The revision of the file you're editing. 
        /// If <paramref name="revision"/> matches the latest version of the file on the user's Dropbox, that file will be replaced.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the uploaded file.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler UploadFileAsync(IResource file, string path, bool overwrite, string revision, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            return this.RestTemplate.ExchangeAsync<Entry>(
                this.BuildUploadUrl(path, overwrite, revision), HttpMethod.PUT, new HttpEntity(file),
                r =>
                {
                    operationCompleted(new RestOperationCompletedEventArgs<Entry>(
                         r.Error == null ? r.Response.Body : null, r.Error, r.Cancelled, r.UserState));
                });
        }

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides an array of bytes containing the file's content.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler DownloadFileAsync(string path, Action<RestOperationCompletedEventArgs<byte[]>> operationCompleted)
        {
            return this.DownloadFileAsync(path, null, operationCompleted);
        }

        /// <summary>
        /// Asynchronously downloads a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file you want to retrieve, relative to root.</param>
        /// <param name="revision">The revision of the file to retrieve.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides an array of bytes containing the file's content.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler DownloadFileAsync(string path, string revision, Action<RestOperationCompletedEventArgs<byte[]>> operationCompleted)
        {
            return this.RestTemplate.GetForObjectAsync<byte[]>(this.BuildDownloadUrl(path, revision), operationCompleted);
        }

        /// <summary>
        /// Asynchronously retrieves file or folder metadata.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetMetadataAsync(string path, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            return this.GetMetadataAsync(path, new MetadataParameters(), operationCompleted);
        }

        /// <summary>
        /// Asynchronously retrieves file or folder metadata. 
        /// May return <see langword="null"/> if an hash is provided through parameters.
        /// </summary>
        /// <param name="path">The Dropbox path to the file or folder, relative to root.</param>
        /// <param name="parameters">The parameters for retrieving file and folder metadata.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetMetadataAsync(string path, MetadataParameters parameters, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            return this.RestTemplate.GetForObjectAsync<Entry>(this.BuildMetadataUrl(path, parameters), operationCompleted);
        }

        /// <summary>
        /// Asynchronously obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a list of metadata <see cref="Entry">entries</see>.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetRevisionsAsync(string path, Action<RestOperationCompletedEventArgs<IList<Entry>>> operationCompleted)
        {
            return this.GetRevisionsAsync(path, 0, operationCompleted);
        }

        /// <summary>
        /// Asynchronously obtains metadata for the previous revisions of a file.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revLimit">
        /// The maximum numbers of revisions to retrieve. Default is 10, Max is 1,000.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a list of metadata <see cref="Entry">entries</see>.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetRevisionsAsync(string path, int revLimit, Action<RestOperationCompletedEventArgs<IList<Entry>>> operationCompleted)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (revLimit > 0)
            {
                parameters.Add("rev_limit", revLimit.ToString());
            }
            this.AddLocaleTo(parameters);
            return this.RestTemplate.GetForObjectAsync<IList<Entry>>(this.BuildUrl("revisions/", path, parameters), operationCompleted);
        }

        /// <summary>
        /// Asynchronously restores a file path to a previous revision.
        /// </summary>
        /// <param name="path">The Dropbox path to the file, relative to root.</param>
        /// <param name="revision">The revision of the file to restore.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a metadata <see cref="Entry"/> for the restored file. 
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler RestoreAsync(string path, string revision, Action<RestOperationCompletedEventArgs<Entry>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            request.Add("rev", revision);
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<Entry>(this.BuildUrl("restore/", path), request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler SearchAsync(string path, string query, Action<RestOperationCompletedEventArgs<IList<Entry>>> operationCompleted)
        {
            return this.SearchAsync(path, query, 0, false, operationCompleted);
        }

        /// <summary>
        /// Asynchronously searches for all files and folders that match the search query.
        /// </summary>
        /// <param name="path">The Dropbox path to the folder you want to search from, relative to root.</param>
        /// <param name="query">The search string. Must be at least three characters long.</param>
        /// <param name="fileLimit">
        /// The maximum numbers of file entries to retrieve. The maximum and default value is 1000.
        /// </param>
        /// <param name="includeDeleted">
        /// If <see langword="true"/>, then files and folders that have been deleted will also be included in the search.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a list of metadata <see cref="Entry">entries</see> for any matching files and folders.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler SearchAsync(string path, string query, int fileLimit, bool includeDeleted, Action<RestOperationCompletedEventArgs<IList<Entry>>> operationCompleted)
        {
            NameValueCollection request = this.BuildSearchParameters(path, query, fileLimit, includeDeleted);
            return this.RestTemplate.PostForObjectAsync<IList<Entry>>(this.BuildUrl("search/", path), request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously creates and returns a shareable link to files or folders.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the file or folder you want a sharable link to, relative to root.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a shareable link to the file or folder.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetShareableLinkAsync(string path, Action<RestOperationCompletedEventArgs<DropboxLink>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<DropboxLink>(this.BuildUrl("shares/", path), request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously returns a link for streaming media files.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the media file you want a direct link to, relative to root.
        /// </param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides a direct link to the media file.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler GetMediaLinkAsync(string path, Action<RestOperationCompletedEventArgs<DropboxLink>> operationCompleted)
        {
            NameValueCollection request = new NameValueCollection();
            this.AddLocaleTo(request);
            return this.RestTemplate.PostForObjectAsync<DropboxLink>(this.BuildUrl("media/", path), request, operationCompleted);
        }

        /// <summary>
        /// Asynchronously downloads a thumbnail for an image.
        /// </summary>
        /// <param name="path">
        /// The Dropbox path to the image file you want to thumbnail, relative to root.
        /// </param>
        /// <param name="format">The image format of the thumbnail to download.</param>
        /// <param name="size">The size of the thumbnail to download.</param>
        /// <param name="operationCompleted">
        /// The <code>Action&lt;&gt;</code> to perform when the asynchronous request completes. 
        /// Provides an array of bytes containing the thumbnail's content.
        /// </param>
        /// <returns>
        /// A <see cref="RestOperationCanceler"/> instance that allows to cancel the asynchronous operation.
        /// </returns>
        /// <exception cref="ApiException">If there is an error while communicating with Dropbox.</exception>
        public RestOperationCanceler DownloadThumbnailAsync(string path, ThumbnailFormat format, ThumbnailSize size, Action<RestOperationCompletedEventArgs<byte[]>> operationCompleted)
        {
            return this.RestTemplate.GetForObjectAsync<byte[]>(this.BuildThumbnailsUrl(path, format, size), operationCompleted);
        }
#endif

        /// <summary>
        /// Gets the underlying <see cref="IRestOperations"/> object allowing for consumption of Dropbox endpoints 
        /// that may not be otherwise covered by the API binding. 
        /// </summary>
        /// <remarks>
        /// The <see cref="IRestOperations"/> object returned is configured to include an OAuth "Authorization" header on all requests.
        /// </remarks>
        public IRestOperations RestOperations
        {
            get { return this.RestTemplate; }
        }

        #endregion

        /// <summary>
        /// Enables customization of the <see cref="RestTemplate"/> used to consume provider API resources.
        /// </summary>
        /// <remarks>
        /// An example use case might be to configure a custom error handler. 
        /// Note that this method is called after the RestTemplate has been configured with the message converters returned from GetMessageConverters().
        /// </remarks>
        /// <param name="restTemplate">The RestTemplate to configure.</param>
        protected override void ConfigureRestTemplate(RestTemplate restTemplate)
        {
            restTemplate.BaseAddress = API_URI_BASE;
        }

        /// <summary>
        /// Returns a list of <see cref="IHttpMessageConverter"/>s to be used by the internal <see cref="RestTemplate"/>.
        /// </summary>
        /// <remarks>
        /// This implementation adds <see cref="SpringJsonHttpMessageConverter"/> and <see cref="ByteArrayHttpMessageConverter"/> to the default list.
        /// </remarks>
        /// <returns>
        /// The list of <see cref="IHttpMessageConverter"/>s to be used by the internal <see cref="RestTemplate"/>.
        /// </returns>
        protected override IList<IHttpMessageConverter> GetMessageConverters()
        {
            JsonMapper jsonMapper = new JsonMapper();
            jsonMapper.RegisterDeserializer(typeof(DropboxProfile), new DropboxProfileDeserializer());
            jsonMapper.RegisterDeserializer(typeof(Entry), new EntryDeserializer());
            jsonMapper.RegisterDeserializer(typeof(IList<Entry>), new EntryListDeserializer());
            jsonMapper.RegisterDeserializer(typeof(DropboxLink), new DropboxLinkDeserializer());

            IList<IHttpMessageConverter> converters = base.GetMessageConverters();
            converters.Add(new ByteArrayHttpMessageConverter());
            converters.Add(new ResourceHttpMessageConverter());
            converters.Add(new SpringJsonHttpMessageConverter(jsonMapper));
            return converters;
        }

        private void AddLocaleTo(NameValueCollection parameters)
        {
            // TODO: locale parameter
        }

        private NameValueCollection BuildSearchParameters(string dropboxPath, string query, int fileLimit, bool includeDeleted)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("query", query);
            if (fileLimit > 0)
            {
                parameters.Add("file_limit", fileLimit.ToString());
            }
            if (includeDeleted)
            {
                parameters.Add("include_deleted", "true");
            }
            this.AddLocaleTo(parameters);
            return parameters;
        }

        private string BuildUploadUrl(string dropboxPath, bool overwrite, string revision)
        {
            NameValueCollection parameters = new NameValueCollection();
            this.AddLocaleTo(parameters);
            if (!overwrite)
            {
                parameters.Add("overwrite", "false");
            }
            if (!String.IsNullOrEmpty(revision))
            {
                parameters.Add("parent_rev", revision);
            }
            return this.BuildUrl("https://api-content.dropbox.com/1/files_put/", dropboxPath, parameters);
        }

        private string BuildDownloadUrl(string dropboxPath, string revision)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (!String.IsNullOrEmpty(revision))
            {
                parameters.Add("rev", revision);
            }
            return this.BuildUrl("https://api-content.dropbox.com/1/files/", dropboxPath, parameters);
        }

        private string BuildThumbnailsUrl(string dropboxPath, ThumbnailFormat format, ThumbnailSize size)
        {
            NameValueCollection parameters = new NameValueCollection();
            parameters.Add("format", format.ToString().ToUpper());
            parameters.Add("size", ThumbnailSizeToString(size));
            return this.BuildUrl("https://api-content.dropbox.com/1/thumbnails/", dropboxPath, parameters);
        }

        private string BuildMetadataUrl(string dropboxPath, MetadataParameters metadataParameters)
        {
            NameValueCollection parameters = new NameValueCollection();
            if (metadataParameters.FileLimit > 0)
            {
                parameters.Add("file_limit", metadataParameters.FileLimit.ToString());
            }
            if (metadataParameters.Hash != null)
            {
                parameters.Add("hash", metadataParameters.Hash);
            }
            if (!metadataParameters.IncludeContents)
            {
                parameters.Add("list", "false");
            }
            if (metadataParameters.IncludeDeleted)
            {
                parameters.Add("include_deleted", "true");
            }
            if (metadataParameters.Revision != null)
            {
                parameters.Add("rev", metadataParameters.Revision);
            }
            return this.BuildUrl("metadata/", dropboxPath, parameters);
        }

        private string BuildUrl(string path, string dropboxPath)
        {
            return this.BuildUrl(path, dropboxPath, new NameValueCollection());
        }

        private string BuildUrl(string path, string dropboxPath, NameValueCollection parameters)
        {
            return this.BuildUrl(path.TrimEnd('/') + "/" + this.root + "/" + dropboxPath.TrimStart('/'), parameters);
        }

        private string BuildUrl(string path, NameValueCollection parameters)
        {
            StringBuilder qsBuilder = new StringBuilder();
            bool isFirst = true;
            foreach (string key in parameters)
            {
                if (isFirst)
                {
                    qsBuilder.Append('?');
                    isFirst = false;
                }
                else
                {
                    qsBuilder.Append('&');
                }
                qsBuilder.Append(HttpUtils.UrlEncode(key));
                qsBuilder.Append('=');
                qsBuilder.Append(HttpUtils.UrlEncode(parameters[key]));
            }
            return path + qsBuilder.ToString();
        }

        private static string ThumbnailSizeToString(ThumbnailSize size)
        {
            switch(size)
            {
                case ThumbnailSize.Small:
                    return "s";
                case ThumbnailSize.Medium:
                    return "m";
                case ThumbnailSize.Large:
                    return "l";
                case ThumbnailSize.ExtraLarge:
                    return "xl";
                default:
                    return "small";
            }
        }
    }
}