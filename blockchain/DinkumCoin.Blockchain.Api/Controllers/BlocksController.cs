using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DinkumCoin.Blockchain.Api.Dto;
using DinkumCoin.Blockchain.Core.Models;
using DinkumCoin.Blockchain.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Blockchain.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlocksController : ControllerBase
    {
        private readonly ILogger<BlocksController> _logger;
        private readonly IBlockchainService _bcService;

        public BlocksController(
            ILogger<BlocksController> logger,
            IBlockchainService bcService
            )
        {
            _bcService = bcService ?? throw new ArgumentNullException(nameof(bcService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        [HttpGet()]
        [ProducesResponseType(typeof(List<Block>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _bcService.GetEntireChain();
            return Ok(result);
        }


        [HttpGet("last")]
        [ProducesResponseType(typeof(BlockProof), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetLastBlock()
        {
            var result = await _bcService.GetLastBlockProof();
            return Ok(result);
        }

        [HttpGet("validate")]
        [ProducesResponseType(typeof(ChainValidationResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ValidateChain()
        {
            bool isValid = await _bcService.ValidateChain();
            return Ok(new ChainValidationResponse { IsValid = isValid });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBlock(SolutionAttempt solution)
        {
            var result = await _bcService.SolveCurrentBlock(solution);
            return result ? Ok() : (IActionResult)BadRequest(new ErrorResponse("Bad solution provided"));
        }


    }
}
