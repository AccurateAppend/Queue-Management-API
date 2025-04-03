#region Legal
/*
Copyright (c) 2015-2025, AccurateAppend Corp
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace Net_8_Sample_code.Models
{
    class Job
    {
        /// <summary>
        /// The unique identifier of the submitted file for appending. 
        /// If submitted via SFTP or via website upload, this is automatically generated for you. 
        /// If submitted via the Submit File endpoint, this is the value supplied in the "request_id" parameter. 
        /// This value is also available as part of the returned data from the View Files endpoint.
        /// </summary>
        public Guid JobKey { get; set; }

        /// <summary>
        /// The name of your submitted list.
        /// </summary>
        public String FileName { get; set; }

        /// <summary>
        /// The date and time your list was submitted (in ISO 8601 format).
        /// </summary>
        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// The date and time your list was completed processing (in ISO 8601 format). 
        /// This element will only be present when the "Status" element value is "Complete".
        /// </summary>
        public DateTime DateComplete { get; set; }

        /// <summary>
        /// Indicates what method was used to submit the file for appending. 
        /// Possible values are Admin, Api, ListBuilder, NationBuilder, PublicWebsite, or SFTP.
        /// </summary>
        public String Source { get; set; }

        /// <summary>
        /// Indicates what the current status of the file is in. 
        /// Possible values are Appending, Complete, or QA.
        /// </summary>
        public String Status { get; set; }

        /// <summary>
        /// The total number of records in the job.
        /// </summary>
        public Int32 RecordCount { get; set; }

        /// <summary>
        /// The count of input records on your file that have one or more matches made (depending on appends performed). 
        /// This number is always between 0 and the number of records in your file, 
        /// not the total number of matches made in total appending.
        /// </summary>
        public Int32 MatchedRecords { get; set; }

        /// <summary>
        /// Provides an evaluation of the weighted average % of records 
        /// that have matches for all appends performed for your list.
        /// </summary>
        public Double AvgMatchRate { get; set; }

        /// <summary>
        /// Contains the Details or Download URLs
        /// </summary>
        public Reference _Ref { get; set; }

    }
}
