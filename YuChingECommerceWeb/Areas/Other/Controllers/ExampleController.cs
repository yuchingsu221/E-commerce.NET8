using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NLog;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YuChingECommerce.DataAccess.Repository.IRepository;

namespace FoodPanda.Backend.API.Controllers
{
    [ProducesResponseType(200)]
    public class ExampleController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;    

        public ExampleController(         
            IUnitOfWork unitOfWork
            )
        {
            _unitOfWork = unitOfWork;        
        }

        #region 舊資料
        /* 
        [HttpGet("test-upload-node")]
        public async Task TestImportNode()
        {
            try
            {
                using (var connectioin = _dapperHelper.GetConnection())
                {
                    var path = "C:\\work\\document\\FoodPanda\\node_for_import_new.csv";
                    var file = new StreamReader(path, System.Text.Encoding.UTF8);
                    string line;
                    var nodeRawNodels = new List<NodeRawModel>();
                    var row = 1;
                    while ((line = file.ReadLine()) != null)
                    {
                        var lineArray = line.Split(',');
                        //if (row == 17)
                        //{
                        //    Console.WriteLine();
                        //}
                        Console.WriteLine($"{row} {line}");
                        var node = new NodeRawModel();
                        node.CategoryNodeId = int.Parse(lineArray[0]);
                        node.CategoryName = lineArray[1];
                        node.ProgramNodeId = int.Parse(lineArray[2]);
                        node.ProgramName = lineArray[3];
                        node.ProgramCode = lineArray[8];
                        node.FirstNodeId = !string.IsNullOrWhiteSpace(lineArray[4]) ? int.Parse(lineArray[4]) : null;
                        node.FirstNodeName = lineArray[5];
                        node.SecondNodeId = !string.IsNullOrWhiteSpace(lineArray[6]) ? int.Parse(lineArray[6]) : null;
                        node.SecondNodeName = lineArray[7];
                        nodeRawNodels.Add(node);
                        row++;
                    }

                    var programCategorySql = $@"select * from program_category;";
                    var programCategory = await connectioin.QueryAsync<ProgramCategory>(programCategorySql);
                    var programCategoryDict = programCategory.ToDictionary(pc => pc.Name, pc => pc.Id);

                    var programSql = $@"select * from program_node_setting where code is not null;";
                    var programs = await connectioin.QueryAsync<ProgramNodeSetting>(programSql);
                    var programCodeDict = programs.ToDictionary(p => p.Code, p => p.Id);

                    var maxProgramNodeSql = $"select max(id) from program_node_setting;";
                    var programNodeId = await connectioin.QueryFirstOrDefaultAsync<int>(maxProgramNodeSql);
                    programNodeId++;

                    var programFromCsv = nodeRawNodels.Where(n => !string.IsNullOrWhiteSpace(n.ProgramCode));
                    var programDictForCsv = programFromCsv.ToDictionary(n => n.ProgramName, n => n.ProgramCode);
                    // recode old node id and new id
                    var nodeIdRecorder = new Dictionary<int, int>();
                    // list for db
                    var programCategoryPrograms = new List<ProgramCategoryProgram>();
                    var programNodes = new List<ProgramNodeSetting>();
                    foreach (var node in nodeRawNodels)
                    {
                        if (node.SecondNodeId != null)
                        {
                            if (!nodeIdRecorder.ContainsKey((int)node.FirstNodeId))
                            {
                                Console.WriteLine($"first node id not found, {node.FirstNodeId}");
                                throw new Exception();
                            }

                            var programNode = new ProgramNodeSetting
                            {
                                Id = programNodeId,
                                ParentId = nodeIdRecorder[(int)node.FirstNodeId],
                                Name = node.SecondNodeName,
                                //OldNodeId = (int)node.SecondNodeId,
                                Status = 1
                            };

                            programNodes.Add(programNode);
                            programNodeId++;
                        }
                        else if (node.FirstNodeId != null)
                        {
                            //if (!programDictForCsv.ContainsKey(node.ProgramName))
                            //{
                            //    Console.WriteLine($"node program name not found, {node.ProgramName}");
                            //    throw new Exception();
                            //}

                            //if (!programNameDict.ContainsKey(node.ProgramName))
                            //{
                            //    Console.WriteLine($"node program name not found, {node.ProgramName}");
                            //    throw new Exception();
                            //}

                            if (!programDictForCsv.ContainsKey(node.ProgramName))
                            {
                                Console.WriteLine($"node program name not found, {node.ProgramName}");
                                throw new Exception();
                            }

                            var programCode = programDictForCsv[node.ProgramName];

                            if (!programCodeDict.ContainsKey(programCode))
                            {
                                Console.WriteLine($"node program code not found, {programCode} {node.ProgramName}");
                                throw new Exception();
                            }

                            var programNode = new ProgramNodeSetting
                            {
                                Id = programNodeId,
                                ParentId = programCodeDict[programCode],
                                Name = node.FirstNodeName,
                                //OldNodeId = (int)node.FirstNodeId,
                                Status = 1
                            };
                            nodeIdRecorder.Add((int)node.FirstNodeId, programNodeId);
                            programNodes.Add(programNode);
                            programNodeId++;
                        }
                        else
                        {
                            //program category and program
                            if (!programCategoryDict.ContainsKey(node.CategoryName))
                            {
                                Console.WriteLine($"program category not found, {node.CategoryName}");
                                throw new Exception();
                            }

                            if (!programCodeDict.ContainsKey(node.ProgramCode))
                            {
                                Console.WriteLine($"program not found, {node.ProgramCode} {node.ProgramName}");
                                throw new Exception();
                            }

                            var programCategoryProgram = new ProgramCategoryProgram
                            {
                                CategoryId = programCategoryDict[node.CategoryName],
                                ProgramId = programCodeDict[node.ProgramCode],
                                Sort = 1,
                            };
                            programCategoryPrograms.Add(programCategoryProgram);
                        }
                    }
                    await _unitOfWork.ProgramCategoryProgram.InsertMultipleAsync(programCategoryPrograms);
                    await _unitOfWork.ProgramNodeSetting.InsertMultipleAsync(programNodes);
                    await _unitOfWork.SaveChangesAsync();
                    file.Close();
                    file.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private class NodeRawModel
        {
            public int CategoryNodeId { get; set; }
            public string CategoryName { get; set; }
            public int ProgramNodeId { get; set; }
            public string ProgramName { get; set; }
            public string ProgramCode { get; set; }
            public int? FirstNodeId { get; set; }
            public string FirstNodeName { get; set; }
            public int? SecondNodeId { get; set; }
            public string SecondNodeName { get; set; }
        }

        private class EpisodeNodeCsv
        {
            public string EpisodeId { get; set; }
            public int NodeId { get; set; }
        }

        private class NodeIdAndName
        {
            public int NodeId { get; set; }
            public string NodeName { get; set; }
        }

        [HttpGet("test-update-program-old-node-id")]
        public async Task TestUpdateProgramOldNodeId()
        {
            using (var connectioin = _dapperHelper.GetConnection())
            {
                var path = "C:\\work\\document\\FoodPanda\\node_for_import_new.csv";
                var file = new StreamReader(path, System.Text.Encoding.UTF8);
                string line;
                var nodeRawNodels = new List<NodeRawModel>();
                var row = 1;
                while ((line = file.ReadLine()) != null)
                {
                    var lineArray = line.Split(',');
                    //if (row == 17)
                    //{
                    //    Console.WriteLine();
                    //}
                    //Console.WriteLine($"{row} {line}");
                    var node = new NodeRawModel();
                    node.CategoryNodeId = int.Parse(lineArray[0]);
                    node.CategoryName = lineArray[1];
                    node.ProgramNodeId = int.Parse(lineArray[2]);
                    node.ProgramName = lineArray[3];
                    node.ProgramCode = lineArray[8];
                    node.FirstNodeId = !string.IsNullOrWhiteSpace(lineArray[4]) ? int.Parse(lineArray[4]) : null;
                    node.FirstNodeName = lineArray[5];
                    node.SecondNodeId = !string.IsNullOrWhiteSpace(lineArray[6]) ? int.Parse(lineArray[6]) : null;
                    node.SecondNodeName = lineArray[7];
                    nodeRawNodels.Add(node);
                    row++;
                }

                var programNodes = nodeRawNodels.Where(n => !string.IsNullOrWhiteSpace(n.ProgramCode));
                var programNodeIdDict = programNodes.ToDictionary(p => p.ProgramCode, p => p.ProgramNodeId);
                var programSql = $@"select * from program_node_setting where code in @code;";
                var programNodeSettings = await connectioin.QueryAsync<ProgramNodeSetting>(programSql, new
                {
                    Code = programNodes.Select(n => n.ProgramCode)
                });
                programNodeSettings = programNodeSettings.Select(
                    p =>
                    {
                        //p.OldNodeId = programNodeIdDict[p.Code];
                        return p;
                    });

                await _unitOfWork.ProgramNodeSetting.UpdateMultipleAsync(programNodeSettings);
                await _unitOfWork.SaveChangesAsync();
                file.Close();
                file.Dispose();
            }
        }

        [HttpGet("test-episode-node")]
        public async Task TestEpisodeNodeOldNodeId()
        {
            try
            {
                var path = "C:\\work\\document\\FoodPanda\\episode_node_fro_import.csv";
                var file = new StreamReader(path, Encoding.UTF8);
                string line;
                var episodeNodesCsv = new List<EpisodeNodeCsv>();
                while ((line = file.ReadLine()) != null)
                {
                    var lineArray = line.Split(',');
                    //Console.WriteLine(line);
                    var episodeNode = new EpisodeNodeCsv
                    {
                        EpisodeId = lineArray[0],
                        NodeId = int.Parse(lineArray[1])
                    };
                    episodeNodesCsv.Add(episodeNode);
                }


                //var episodeSkipPath = @"C:\work\document\FoodPanda\not_found_episode_of_node_20220302.csv";
                //file = new StreamReader(episodeSkipPath, Encoding.UTF8);
                //var skipEpisodes = new HashSet<string>();
                //while ((line = file.ReadLine()) != null)
                //{
                //    //Console.WriteLine(line);
                //    skipEpisodes.Add(line);
                //}
                //path = "C:\\work\\document\\FoodPanda\\nodeId.csv";
                //var nodeIdAndNames = new List<NodeIdAndName>();
                //while ((line = file.ReadLine()) != null)
                //{
                //    var lineArray = line.Split(',');
                //    var nodeIdAndName = new NodeIdAndName
                //    {
                //        NodeId = int.Parse(lineArray[0]),
                //        NodeName = lineArray[1],
                //    };
                //    nodeIdAndNames.Add(nodeIdAndName);
                //}


                using (var connection = _dapperHelper.GetConnection())
                {
                    var nodeSql = $@"select * from program_node_setting where old_node_id is not null;";
                    var nodes = await connection.QueryAsync<ProgramNodeSetting>(nodeSql);
                    //var newNodeDict = nodes.ToDictionary(n => n.Name, n => n.Id);
                    //var oldNodeDict = nodeIdAndNames.ToDictionary(n => n.NodeId, n => n.NodeName);
                    //var nodeDict = nodes.ToDictionary(n => n.OldNodeId);
                    var episodeCodeIdSql = $@"select id, episode_id, episode_name_vod from episode_setting;";
                    var episodes = await connection.QueryAsync<EpisodeSetting>(episodeCodeIdSql);
                    var episodeDict = episodes.ToDictionary(e => e.EpisodeId, e => new { e.Id, e.EpisodeNameVod });

                    var episodeSettingNodes = new List<EpisodeSettingNode>();
                    var episodeNodeCsvGroups = episodeNodesCsv.GroupBy(e => e.NodeId);
                    var episodeNotFoundIds = new List<string>();
                    foreach (var episodeNodeCsvGroup in episodeNodeCsvGroups)
                    {

                        var sort = 1;
                        foreach (var episodeNodeCsv in episodeNodeCsvGroup)
                        {
                            //if (!nodeDict.ContainsKey(episodeNodeCsv.NodeId))
                            //{
                            //    Console.WriteLine($"node id not found, {episodeNodeCsv.NodeId}");
                            //    throw new Exception();
                            //}

                            //if (!episodeDict.ContainsKey(episodeNodeCsv.EpisodeId))
                            //{
                            //    episodeNotFoundIds.Add(episodeNodeCsv.EpisodeId);
                            //    continue;
                            //}
                            ////if (skipEpisodes.Contains(episodeNodeCsv.EpisodeId))
                            ////{
                            ////    // 略過
                            ////    continue;
                            ////}

                            //// Food Panda node id 轉換成我們的id
                            //var newNode = nodeDict[episodeNodeCsv.NodeId];
                            //var episodeInfo = episodeDict[episodeNodeCsv.EpisodeId];
                            //var episodeSettingNode = new EpisodeSettingNode
                            //{
                            //    EpisodeId = episodeInfo.Id,
                            //    DisplayName = episodeInfo.EpisodeNameVod,
                            //    NodeId = newNode.Id,
                            //    Sort = sort,
                            //};
                            //episodeSettingNodes.Add(episodeSettingNode);
                            //sort++;
                        }
                    }

                    //var nameIsNull = episodeSettingNodes.Where(e => string.IsNullOrWhiteSpace(e.DisplayName));

                    Console.WriteLine("episode not found:");
                    Console.WriteLine(string.Join(",", episodeNotFoundIds));
                    var notFoundCsvContent = new StringBuilder();
                    foreach (var id in episodeNotFoundIds)
                    {
                        notFoundCsvContent.AppendLine(id);
                    }

                    var notFoundCsvPath = @"C:\work\document\FoodPanda\not_found_episode_of_node_20220302.csv";
                    using (var notFoundCsv = new StreamWriter(notFoundCsvPath))
                    {
                        await notFoundCsv.WriteLineAsync(notFoundCsvContent);
                    }

                    var splitNodes = new List<List<EpisodeSettingNode>>();
                    var batchSaveEpisodeNode = new List<EpisodeSettingNode>();
                    foreach (var en in episodeSettingNodes)
                    {
                        batchSaveEpisodeNode.Add(en);
                        if (batchSaveEpisodeNode.Count == 1000)
                        {
                            splitNodes.Add(batchSaveEpisodeNode);
                            batchSaveEpisodeNode = new List<EpisodeSettingNode>();
                        }
                    }

                    if (batchSaveEpisodeNode.Count > 0)
                    {
                        splitNodes.Add(batchSaveEpisodeNode);
                    }

                    var count = 0;
                    foreach (var saveNodes in splitNodes)
                    {
                        //var options = new DbContextOptionsBuilder<FoodPandaContext>().UseMySql(_configuration.GetConnectionString("MySql"), ServerVersion.Parse("8.0.23-mysql")).Options;

                        //using (var context = new FoodPandaContext(options, _configuration))
                        //{
                        //    await context.EpisodeSettingNodes.AddRangeAsync(saveNodes);
                        //    //await context.SaveChangesAsync();
                        //    count += 1000;
                        //    Console.WriteLine($"save, {count}");
                        //}

                        await _unitOfWork.EpisodeSettingNode.InsertMultipleAsync(saveNodes);
                        count += 1000;
                        Console.WriteLine($"save, {count}");
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
                file.Close();
                file.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        [HttpGet("test-episode-node2")]
        public async Task TestEpisodeNodeNewNodeId()
        {
            try
            {
                var path = @"C:\work\document\FoodPanda\影片無節點(匯入)2022_0317_Y.csv";
                var file = new StreamReader(path, Encoding.UTF8);
                string line;
                var episodeNodesCsv = new List<EpisodeNodeCsv>();
                while ((line = file.ReadLine()) != null)
                {
                    //Console.WriteLine(line);
                    var lineArray = line.Split(',');
                    var nodeIdString = lineArray[1];
                    if (string.IsNullOrWhiteSpace(nodeIdString))
                        continue;
                    //Console.WriteLine(line);
                    var episodeNode = new EpisodeNodeCsv
                    {
                        EpisodeId = lineArray[0],
                        NodeId = int.Parse(lineArray[1])
                    };
                    episodeNodesCsv.Add(episodeNode);
                }

                file.Close();
                file.Dispose();
                //var episodeSkipPath = @"C:\work\document\FoodPanda\not_found_episode_of_node_20220302.csv";
                //file = new StreamReader(episodeSkipPath, Encoding.UTF8);
                //var skipEpisodes = new HashSet<string>();
                //while ((line = file.ReadLine()) != null)
                //{
                //    //Console.WriteLine(line);
                //    skipEpisodes.Add(line);
                //}
                //path = "C:\\work\\document\\FoodPanda\\nodeId.csv";
                //var nodeIdAndNames = new List<NodeIdAndName>();
                //while ((line = file.ReadLine()) != null)
                //{
                //    var lineArray = line.Split(',');
                //    var nodeIdAndName = new NodeIdAndName
                //    {
                //        NodeId = int.Parse(lineArray[0]),
                //        NodeName = lineArray[1],
                //    };
                //    nodeIdAndNames.Add(nodeIdAndName);
                //}


                using (var connection = _dapperHelper.GetConnection())
                {
                    var nodeSql = $@"select * from program_node_setting;";
                    var nodes = await connection.QueryAsync<ProgramNodeSetting>(nodeSql);
                    //var newNodeDict = nodes.ToDictionary(n => n.Name, n => n.Id);
                    //var oldNodeDict = nodeIdAndNames.ToDictionary(n => n.NodeId, n => n.NodeName);
                    var nodeDict = nodes.ToDictionary(n => n.Id);
                    var episodeCodeIdSql = $@"select id, episode_id, episode_name_vod from episode_setting;";
                    var episodes = await connection.QueryAsync<EpisodeSetting>(episodeCodeIdSql);
                    var episodeDict = episodes.ToDictionary(e => e.EpisodeId, e => new { e.Id, e.EpisodeNameVod });

                    var episodeSettingNodes = new List<EpisodeSettingNode>();
                    var episodeNodeCsvGroups = episodeNodesCsv.GroupBy(e => e.NodeId);
                    var episodeNotFoundIds = new List<string>();
                    foreach (var episodeNodeCsvGroup in episodeNodeCsvGroups)
                    {

                        var sort = 1;
                        foreach (var episodeNodeCsv in episodeNodeCsvGroup)
                        {
                            if (!nodeDict.ContainsKey(episodeNodeCsv.NodeId))
                            {
                                Console.WriteLine($"node id not found, {episodeNodeCsv.NodeId}");
                                throw new Exception();
                            }

                            if (!episodeDict.ContainsKey(episodeNodeCsv.EpisodeId))
                            {
                                episodeNotFoundIds.Add(episodeNodeCsv.EpisodeId);
                                continue;
                            }
                            //if (skipEpisodes.Contains(episodeNodeCsv.EpisodeId))
                            //{
                            //    // 略過
                            //    continue;
                            //}

                            var episodeInfo = episodeDict[episodeNodeCsv.EpisodeId];
                            var episodeSettingNode = new EpisodeSettingNode
                            {
                                EpisodeId = episodeInfo.Id,
                                DisplayName = episodeInfo.EpisodeNameVod,
                                NodeId = episodeNodeCsv.NodeId,
                                Sort = sort,
                            };
                            episodeSettingNodes.Add(episodeSettingNode);
                            sort++;
                        }
                    }

                    //var nameIsNull = episodeSettingNodes.Where(e => string.IsNullOrWhiteSpace(e.DisplayName));

                    Console.WriteLine("episode not found:");
                    Console.WriteLine(string.Join(",", episodeNotFoundIds));
                    var notFoundCsvContent = new StringBuilder();
                    foreach (var id in episodeNotFoundIds)
                    {
                        notFoundCsvContent.AppendLine(id);
                    }

                    var notFoundCsvPath = @"C:\work\document\FoodPanda\not_found_episode_of_node_20220318.csv";
                    using (var notFoundCsv = new StreamWriter(notFoundCsvPath))
                    {
                        await notFoundCsv.WriteLineAsync(notFoundCsvContent);
                    }

                    //var splitNodes = new List<List<EpisodeSettingNode>>();
                    //var batchSaveEpisodeNode = new List<EpisodeSettingNode>();
                    //foreach (var en in episodeSettingNodes)
                    //{
                    //    batchSaveEpisodeNode.Add(en);
                    //    if (batchSaveEpisodeNode.Count == 1000)
                    //    {
                    //        splitNodes.Add(batchSaveEpisodeNode);
                    //        batchSaveEpisodeNode = new List<EpisodeSettingNode>();
                    //    }
                    //}

                    //if (batchSaveEpisodeNode.Count > 0)
                    //{
                    //    splitNodes.Add(batchSaveEpisodeNode);
                    //}

                    var splitNodes = episodeSettingNodes.ChunkBy(1000);
                    var count = 0;
                    foreach (var saveNodes in splitNodes)
                    {
                        //var options = new DbContextOptionsBuilder<FoodPandaContext>().UseMySql(_configuration.GetConnectionString("MySql"), ServerVersion.Parse("8.0.23-mysql")).Options;

                        //using (var context = new FoodPandaContext(options, _configuration))
                        //{
                        //    await context.EpisodeSettingNodes.AddRangeAsync(saveNodes);
                        //    //await context.SaveChangesAsync();
                        //    count += 1000;
                        //    Console.WriteLine($"save, {count}");
                        //}

                        await _unitOfWork.EpisodeSettingNode.InsertMultipleAsync(saveNodes);
                        count += 1000;
                        Console.WriteLine($"save, {count}");
                    }
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        /// <summary>
        /// 匯出無節點的影片資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-export-node-csv")]
        public async Task ExportNodeCsv()
        {
            using (var connection = _dapperHelper.GetConnection())
            {
                var sql = $@"
                    select 
                    ep.episode_id,
                    ep.episode_name
                    from (select * from episode_setting where status = 1) e
                    left join episode_setting_node n on n.episode_id = e.id
                    join episode_setting_pms ep on ep.episode_id = e.episode_id
                    where n.id is null;  ";

                var data = await connection.QueryAsync<EpisodeExport>(sql);
                var stringBuilder = new StringBuilder();

                foreach (var d in data)
                {
                    stringBuilder.AppendLine($"{d.EpisodeId}, \"{d.EpisodeName}\"");
                }

                var path = @"C:\work\document\FoodPanda\影片無節點2022_0316.csv";
                using (var csv = new StreamWriter(path))
                {
                    await csv.WriteLineAsync(stringBuilder);
                }
            }
        }

        private class EpisodeExport
        {
            public string EpisodeId { get; set; }
            public string EpisodeName { get; set; }
        }

        /// <summary>
        /// 匯出全部節點影片資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-export-all-node-episode-csv")]
        public async Task ExportEpisodeNodeCsv()
        {
            using (var connection = _dapperHelper.GetConnection())
            {
                var sql = $@"
                    select
                    ep.episode_id,
                    ep.episode_name,
                    n.id new_node_id,
                    n.old_node_id,
                    n.name
                    from episode_setting_node en 
                    join episode_setting e on e.id = en.episode_id AND e.deleted = 0
                    join episode_setting_pms ep on ep.episode_id = e.episode_id
                    join program_node_setting n on n.id = en.node_id 
                    order by en.node_id , en.sort;"
                    ;

                //var sql = $@"
                //    select
                //    e.episode_id,
                //    ep.speaker,
                //    e.status episode_status,
                //    p.code program_code,
                //    p.name program_name,
                //    p.status progeam_status,
                //    p.deleted program_deleted,
                //    e.episode_name_vod,
                //    n.id node_id,
                //    n.name node_name,
                //    n.status node_status,
                //    n.deleted node_deleted
                //    from (select * from episode_setting) e 
                //    join episode_setting_pms ep on ep.episode_id = e.episode_id
                //    join (select * from program_node_setting where code is not null) p on ep.program_id_pms = p.code
                //    left join episode_setting_node en on en.episode_id = e.id
                //    left join (select * from program_node_setting where code is null) n on n.id = en.node_id     
                //    ";

                //sql = $@"
                //    select
                //    e.episode_id,
                //    ep.speaker,
                //    e.status episode_status,
                //    p.code program_code,
                //    p.name program_name,
                //    p.status progeam_status,
                //    p.deleted program_deleted,
                //    e.episode_name_vod,
                //    n.id node_id,
                //    n.name node_name,
                //    n.status node_status,
                //    n.deleted node_deleted
                //    from (select * from episode_setting) e 
                //    join episode_setting_pms ep on ep.episode_id = e.episode_id
                //    join (select * from program_node_setting where code = 'MV11001') p on ep.program_id_pms = p.code
                //    join (select * from episode_setting_node where deleted = false) en on en.episode_id = e.id
                //    join (select * from program_node_setting) n on n.id = en.node_id;   
                //    ";

                //sql = $@"
                //    select
                //    e.episode_id,
                //    ep.speaker,
                //    e.status episode_status,
                //    p.code program_code,
                //    p.name program_name,
                //    p.status progeam_status,
                //    p.deleted program_deleted,
                //    e.episode_name_vod,
                //    n.id node_id,
                //    n.name node_name,
                //    n.status node_status,
                //    n.deleted node_deleted
                //    from (select * from episode_setting) e 
                //    join episode_setting_pms ep on ep.episode_id = e.episode_id
                //    join (select * from program_node_setting where code is not null) p on ep.program_id_pms = p.code
                //    join (select * from episode_setting_node where deleted is false) en on en.episode_id = e.id
                //    join (select * from program_node_setting where code is not null) n on n.id = en.node_id     ;  
                //    ";

                var data = await connection.QueryAsync<NodeExport>(sql);
                var stringBuilder = new StringBuilder();

                foreach (var d in data)
                {
                    stringBuilder.AppendLine($"{d.EpisodeId}, {d.Speaker}, {d.ProgramCode}, \"{d.ProgramName}\", {StatusConvert(d.ProgramStatus)}, {StatusConvert(d.ProgramDeleted)},\"{d.EpisodeNameVod}\", {StatusConvert(d.EpisodeStatus)},{d.NodeId}, \"{d.NodeName}\", {StatusConvert(d.NodeStatus)}, {StatusConvert(d.NodeDeleted)}");
                }

                var path = @"C:\work\document\FoodPanda\影片分類(節點)對應_20220322.csv";
                using (var csv = new StreamWriter(path))
                {
                    await csv.WriteLineAsync(stringBuilder);
                }
            }
        }

        /// <summary>
        /// 匯出節目階層影片資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-export-all-node-episode-csv2")]
        public async Task ExportEpisodeNodeCsv2()
        {
            using (var connection = _dapperHelper.GetConnection())
            {
                var programSql = $@"
                    select * from program_node_setting where code = 'MV11001';"
                    ;

                var program = await connection.QueryFirstAsync<ProgramNodeSetting>(programSql);

                var allNodeIds = new List<int>();
                allNodeIds.Add(program.Id);

                var lastLayerNodeIds = new List<int>();
                lastLayerNodeIds.Add(program.Id);
                var continuedQuery = true;

                while (continuedQuery)
                {
                    var nodeSql = $@"
                    select * from program_node_setting where parent_id in @ids;
                    ";
                    var nodes = await connection.QueryAsync<ProgramNodeSetting>(nodeSql, new { ids = lastLayerNodeIds });

                    if (nodes.Any())
                    {
                        allNodeIds.AddRange(nodes.Select(n => n.Id));
                        lastLayerNodeIds = nodes.Select(n => n.Id).ToList();
                    }
                    else
                    {
                        continuedQuery = false;
                    }
                }

                var sql = $@"
                     select
                    e.episode_id,
                    ep.speaker,
                    e.status episode_status,
                    p.code program_code,
                    p.name program_name,
                    p.status progeam_status,
                    p.deleted program_deleted,
                    e.episode_name_vod,
                    n.id node_id,
                    n.name node_name,
                    n.status node_status,
                    n.deleted node_deleted
                    from (select * from episode_setting_node where deleted = false and node_id in @allNodeIds) en
                    join (select * from episode_setting) e on e.id = en.episode_id
					join episode_setting_pms ep on ep.episode_id = e.episode_id
					join (select * from program_node_setting) p on ep.program_id_pms = p.code
                     join (select * from program_node_setting) n on n.id = en.node_id
                    order by en.node_id, en.sort
                    ";

                var data = await connection.QueryAsync<NodeExport>(sql, new { allNodeIds });
                var stringBuilder = new StringBuilder();

                foreach (var d in data)
                {
                    stringBuilder.AppendLine($"{d.EpisodeId}, {d.Speaker}, {d.ProgramCode}, \"{d.ProgramName}\", {StatusConvert(d.ProgramStatus)}, {StatusConvert(d.ProgramDeleted)},\"{d.EpisodeNameVod}\", {StatusConvert(d.EpisodeStatus)},{d.NodeId}, \"{d.NodeName}\", {StatusConvert(d.NodeStatus)}, {StatusConvert(d.NodeDeleted)}");
                }

                var path = @"C:\work\document\FoodPanda\MV11001底下分類影片對應_20220323.csv";
                using (var csv = new StreamWriter(path))
                {
                    await csv.WriteLineAsync(stringBuilder);
                }
            }
        }

        private string StatusConvert(bool? value)
        {
            if (!value.HasValue)
                return string.Empty;

            return value.Value ? "Y" : "N";
        }

        private class NodeExport
        {
            public string EpisodeId { get; set; }
            public string Speaker { get; set; }
            /// <summary>
            /// 節目代號
            /// </summary>
            public string ProgramCode { get; set; }
            public string ProgramName { get; set; }
            public bool ProgramStatus { get; set; }
            public bool ProgramDeleted { get; set; }
            public string EpisodeNameVod { get; set; }
            public bool EpisodeStatus { get; set; }
            public int? NodeId { get; set; }
            public string NodeName { get; set; }
            public bool? NodeStatus { get; set; }
            public bool? NodeDeleted { get; set; }
        }

        /// <summary>
        /// 匯出全部節點階層資料
        /// </summary>
        /// <returns></returns>
        [HttpGet("test-export-all-node-csv")]
        public async Task ExportAllNodeCsv()
        {
            using (var connection = _dapperHelper.GetConnection())
            {
                var sql = $@"
                    select
                    p.id new_program_id, 
                    p.old_node_id program_old_node_id,
                    p.code,
                    p.name program_name,
                    n1.id new_n1_id,
                    n1.old_node_id n1_old_node_id,
                    n1.name n1_name,
                    n1.sort,
                    n2.id new_n2_id,
                    n2.old_node_id n2_old_node_id,
                    n2.name n2_name,
                    n2.sort
                    from (select * from program_node_setting where code is not null and deleted = false) p
                    left join program_node_setting n1 on n1.parent_id = p.id
                    left join program_node_setting n2 on n2.parent_id = n1.id
                    order by new_program_id, n1.sort, n2.sort;
                    ";

                var data = await connection.QueryAsync<NodeRelationExport>(sql);
                var stringBuilder = new StringBuilder();

                foreach (var d in data)
                {
                    stringBuilder.AppendLine($"{HandleCsvInt(d.NewProgramId)}, {d.Code}, \"{d.ProgramName}\", {HandleCsvInt(d.NewN1Id)},  {d.N1Name}, {HandleCsvInt(d.NewN2Id)}, {d.N2Name}");
                }

                var path = @"C:\work\document\FoodPanda\節目分類(節點)階層_20220321.csv";
                using (var csv = new StreamWriter(path))
                {
                    await csv.WriteLineAsync(stringBuilder);
                }
            }
        }

        private string HandleCsvInt(int value) => value == 0 ? " " : value.ToString();

        private class NodeRelationExport
        {
            public int NewProgramId { get; set; }
            //public int ProgramOldNodeId { get; set; }
            public string Code { get; set; }
            public string ProgramName { get; set; }
            public int NewN1Id { get; set; }
            //public int N1OldNodeId { get; set; }
            public string N1Name { get; set; }
            public int NewN2Id { get; set; }
            //public int N2OldNodeId { get; set; }
            public string N2Name { get; set; }
        }

        [HttpGet("test-fix-playlist-series-episode-sort")]
        public async Task FixPlaylistSeriesEpisodeSort()
        {

            using (var connection = _dapperHelper.GetConnection())
            {

                var playListSql = $@"
                select * from playlist_setting_series
                ";

                var playlists = (await connection.QueryAsync<PlaylistSettingSeries>(playListSql)).ToList();

                var logSql = $@"
                    select 
                    *
                    from playlist_series_log where id in (
                    select 
                    max(p.id)
                    from (select * from playlist_series_log where type != 3) p
                    group by p.playlist_series_id);
                    ";
                var logs = (await connection.QueryAsync<PlaylistSeriesLog>(logSql)).ToList();
                var logDict = logs.ToDictionary(l => l.PlaylistSeriesId, l => l.Episode);

                var episodeDictSql = $@"
                    select id, episode_id from episode_setting;
                    ";

                var episodes = (await connection.QueryAsync<EpisodeIdAndCode>(episodeDictSql)).ToList();
                var episodeDict = episodes.ToDictionary(e => e.EpisodeId, e => e.Id);

                var playlistEpisodes = new List<PlaylistSettingSeriesEpisode>();
                foreach (var p in playlists)
                {
                    try
                    {
                        var deletedPlaylistEpisodeSql = $@"delete from playlist_setting_series_episode where playlist_id = @playlistId;";
                        // 刪除舊的關聯
                        await connection.ExecuteAsync(deletedPlaylistEpisodeSql, new { playlistId = p.Id });
                        // 影片代號
                        var episodeCodes = SplitString(logDict[p.Code]);
                        var sort = 1;
                        episodeCodes.ToList().ForEach(e =>
                            {
                                var pe = new PlaylistSettingSeriesEpisode
                                {
                                    PlaylistId = p.Id,
                                    EpisodeId = episodeDict[e],
                                    Sort = sort,
                                };
                                playlistEpisodes.Add(pe);
                                sort++;
                            });

                        Console.WriteLine($"playlist, id {p.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"error {p.Id},  {ex}");
                    }

                }
                await _unitOfWork.PlaylistSettingSeriesEpisode.InsertMultipleAsync(playlistEpisodes);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        private class EpisodeIdAndCode
        {
            public int Id { get; set; }
            public string EpisodeId { get; set; }
        }

        private IEnumerable<string> SplitString(string value, bool splitByComma = false)
        {
            var splits = splitByComma ? value.Split("、") : value.Split(",");
            return splits.Select(s => s.Trim());
        }

        [HttpGet("test-Captcha")]
        public IActionResult GetCaptchaImage()
        {
            var captchaService = new CaptchaService();
            var captchaCode = "58627"; // This should be generated randomly in real scenario
            var captchaResult = captchaService.GenerateCaptchaImage(120, 40, captchaCode);

            return File(captchaResult.CaptchaByteData, "image/png");
        }

        public class CaptchaResult
        {
            public string CaptchaCode { get; set; }
            public byte[] CaptchaByteData { get; set; }
            public string CaptchaBase64Data => Convert.ToBase64String(CaptchaByteData);
            public string CaptchaImageUrl => $"data:image/png;base64,{CaptchaBase64Data}";
        }

        public class CaptchaService
        {
            public CaptchaResult GenerateCaptchaImage(int width, int height, string captchaCode)
            {
                using var bitmap = new Bitmap(width, height);
                using var graphics = Graphics.FromImage(bitmap);
                var fontName = FontFamily.Families.Any(f => f.Name == "Gaoel") ? "Gaoel" : "Arial";
                using var font = new Font(fontName, 20, FontStyle.Bold);
                graphics.Clear(Color.White);

                var random = new Random();
                // 定義更廣泛的背景點顏色
                Color[] dotColors = { ColorTranslator.FromHtml("#095728"), ColorTranslator.FromHtml("#494c19"),
                          ColorTranslator.FromHtml("#164130"), ColorTranslator.FromHtml("#2d451d"),
                          ColorTranslator.FromHtml("#184334") };
                // 繪製更小的背景點
                for (int i = 0; i < width * height / 50; i++) // 根據圖片大小調整點的總數
                {
                    int x = random.Next(width);
                    int y = random.Next(height);
                    var dotColor = dotColors[random.Next(dotColors.Length)];
                    graphics.FillEllipse(new SolidBrush(dotColor), x, y, 1, 1); // 繪製更小的點   
                }

                // 定義原始深綠色顏色範圍，每個數字使用不同的深綠色調
                Color[] numberColors = { ColorTranslator.FromHtml("#095728"), ColorTranslator.FromHtml("#494c19"),
                          ColorTranslator.FromHtml("#164130"), ColorTranslator.FromHtml("#2d451d"),
                          ColorTranslator.FromHtml("#184334") };
                float xPosition = 10.0f; // 初始x位置
                foreach (char c in captchaCode)
                {
                    var textColor = numberColors[random.Next(numberColors.Length)];
                    graphics.DrawString(c.ToString(), font, new SolidBrush(textColor), new PointF(xPosition, 10));
                    xPosition += font.Size; // 根據字體大小調整下一個字符的位置
                }

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);

                return new CaptchaResult
                {
                    CaptchaCode = captchaCode,
                    CaptchaByteData = ms.ToArray()
                };
            }
        }

        [HttpGet("get_clientIp")]
        public string GetClientIp()
        {
            return GetClientIpAddress(true);
        }

        protected string GetClientIpAddress(bool havePort = false)
        {
            var clientAddress = string.Empty;

            try
            {
                var xForwardedFor = "49.130.129.251:890908, 203.66.35.105:32380";

                if (string.IsNullOrEmpty(xForwardedFor))
                {
                    clientAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                }
                else
                {
                    var ips = xForwardedFor.Split(',').Select(ip => ip.Trim()).ToArray();

                    clientAddress = ips[0];
                    int portIndex = clientAddress.IndexOf(":");
                    if (!havePort && portIndex != -1)
                    {
                        // Remove port number
                        clientAddress = clientAddress.Substring(0, portIndex);
                    }
                }
                Console.WriteLine($"x-Forwarded-For: {xForwardedFor}");
                Console.WriteLine($"clientAddress: {clientAddress}");
            }
            catch (Exception ex) when (ex is Exception)
            {
                Console.WriteLine("Get Client ip error", ex);
            }

            return clientAddress;
        } */
        #endregion
    }

}
