using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DattingApp.API.Data;
using DattingApp.API.Dtos;
using DattingApp.API.Helpers;
using DattingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DattingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userID}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;

        }
        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userID, int id)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo == null)
                return NotFound();
            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessageForUser(int userID,[FromQuery]MessageParams messageParams){
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            messageParams.UserID = userID;
            var messageFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);
            Response.AddPagination(messageFromRepo.CurrentPage, messageFromRepo.PageSize, messageFromRepo.TotalCount, messageFromRepo.TotalPages);
            return Ok(messages);
        }
        
        [HttpGet("thread/{recipientID}")]
        public async Task<IActionResult> GetMessageThread(int userID, int recipientID){
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessageThread(userID,recipientID);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);
            return Ok(messageThread);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userID, MessageForCreationDto messageForCreationDto){
            var sender = await _repo.GetUser(userID, false);
            if(sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            messageForCreationDto.SenderID = userID;
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientID, false);
            if(recipient == null)
                return BadRequest("Could not find user");
            var message = _mapper.Map<Message>(messageForCreationDto);
            _repo.Add(message);
            if(await _repo.SaveAll()){
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new {id = message.ID},messageToReturn);
            }
            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userID)
        {
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo.SenderID == userID)
                messageFromRepo.SenderDeleted = true;
            if(messageFromRepo.RecipientID == userID)
                messageFromRepo.RecipientDelete = true;
            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDelete)
                _repo.Delete(messageFromRepo);
            if(await _repo.SaveAll())
                return NoContent();
            throw new Exception("Error deleting the message");
        }
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userID, int id){
            if(userID != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var message = await _repo.GetMessage(id);
            if(message.RecipientID != userID)
                return Unauthorized();
            message.IsRead = true;
            message.DateRead = DateTime.Now;
            await _repo.SaveAll();
            return NoContent();
        }
    }
}