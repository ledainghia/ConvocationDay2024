﻿using Azure.Core;
using FA23_Convocation2023_API.DTO;
using FA23_Convocation2023_API.Models;
using Microsoft.EntityFrameworkCore;

namespace FA23_Convocation2023_API.Services
{
    public class CheckInService
    {
        private readonly Convo24Context _context = new Convo24Context();

        //update checkin status
        public async Task<string> UpdateCheckinAsync(CheckinRequest checkinRequest)
        {
            // Kiểm tra xem có nhiều bản ghi trùng lặp hay không
            var bachelorDuplicate = await _context.Bachelors
                .Where(b => b.StudentCode == checkinRequest.StudentCode)
                .ToListAsync();

            if (bachelorDuplicate.Count > 1)
            {
                // Tìm bản ghi trùng lặp mà có HallId là null
                var bachelorDuplicateLastCreate = await _context.Bachelors
                    .FirstOrDefaultAsync(b => b.StudentCode == checkinRequest.StudentCode && b.HallId == null);

                // Chỉ xóa nếu tìm thấy bản ghi trùng lặp
                if (bachelorDuplicateLastCreate != null)
                {
                    _context.Bachelors.Remove(bachelorDuplicateLastCreate);
                }
            }

            var bachelor = await _context.Bachelors
                .FirstOrDefaultAsync(b => b.StudentCode == checkinRequest.StudentCode);

            if (bachelor != null && bachelor.StatusBaChelor == "Current")
            {
                throw new Exception("Bachelor đang được chiếu trên màn hình, không thể cập nhật status checkin ngay lúc này");
            }

            var statusCheckin = await _context.CheckIns
                .FirstOrDefaultAsync(c => c.HallId == bachelor.HallId && c.SessionId == bachelor.SessionId);

            if (statusCheckin?.Status == true)
            {
                bachelor.TimeCheckIn = DateTime.Now;
                bachelor.CheckIn = checkinRequest.Status;
                if (bachelor.CheckIn == true)
                {
                    bachelor.Status = true;
                }
                else
                {
                    bachelor.Status = false;
                }

                _context.Bachelors.Update(bachelor);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Cập nhật thất bại, không thể checkin!");
            }

            return "Checkin thành công!";
        }

        //get all checkin
        public async Task<List<CheckIn>> GetAllCheckinAsync()
        {
            return await _context.CheckIns.ToListAsync();
        }

        //uncheck all checkin
        public async Task<string> UncheckAllCheckinAsync()
        {
            foreach (var bachelor in _context.Bachelors)
            {
                bachelor.TimeCheckIn = null;
                bachelor.CheckIn = false;
                bachelor.Status = false;
                _context.Bachelors.Update(bachelor);
            }
            await _context.SaveChangesAsync();
            return "Uncheck all checkin success";
        }

        //get all status checkin
        public async Task<List<CheckInDTO>> GetAllStatusCheckinAsync()
        {
            var checkins = await _context.CheckIns.ToListAsync();
            var result = new List<CheckInDTO>();
            foreach (var check in checkins)
            {
                var hall = await _context.Halls.FirstOrDefaultAsync(h => h.HallId == check.HallId);
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == check.SessionId);
                result.Add(new CheckInDTO
                {
                    CheckinId = check.CheckinId,
                    HallName = hall.HallName,
                    SessionNum = session.Session1,
                    Status = check.Status
                });
            }
            return result;
        }

        //get status checkin
        public async Task<CheckIn> UpdateStatusCheckinAsync(int hallId, int sessionId, bool status)
        {
            var statusCheckin = await _context.CheckIns.FirstOrDefaultAsync(
                c => c.HallId == hallId && c.SessionId == sessionId);
            statusCheckin.Status = status;
            //if status == fasle, get all bacchelor by hallName and sessionNum and find all bachelor have checkin = false and create new list bachelor by list bachelor just found which same infor but hallname and sessionnum == null
            //if (statusCheckin.Status == false)
            //{
            //    var bachelors = await _context.Bachelors.Where(b => b.HallId == statusCheckin.HallId && b.SessionId == statusCheckin.SessionId && b.CheckIn == false).ToListAsync();
            //    foreach (var bachelor in bachelors)
            //    {
            //        var newBachelor = new Bachelor
            //        {
            //            StudentCode = bachelor.StudentCode,
            //            FullName = bachelor.FullName,
            //            Mail = bachelor.Mail,
            //            Faculty = bachelor.Faculty,
            //            Major = bachelor.Major,
            //            Image = bachelor.Image,
            //            Status = bachelor.Status,
            //            StatusBaChelor = bachelor.StatusBaChelor,
            //            HallId = null,
            //            SessionId = null,
            //            Chair = bachelor.Chair,
            //            ChairParent = bachelor.ChairParent,
            //            CheckIn = false,
            //            TimeCheckIn = null
            //        };
            //        await _context.Bachelors.AddAsync(newBachelor);
            //    }
            //}
            _context.CheckIns.Update(statusCheckin);
            await _context.SaveChangesAsync();
            return statusCheckin;
        }
        //get count checkin
        public async Task<List<CheckinSession>> GetCountCheckinAsync()
        {
            List<CheckinSession> result = new();
            foreach (var hallSession in _context.CheckIns)
            {
                var bachelorSession = await _context.Bachelors.Where(b => b.HallId == hallSession.HallId &&
                b.SessionId == hallSession.SessionId).ToListAsync();
                var bachelorCheckined = await _context.Bachelors.Where(b => b.HallId == hallSession.HallId &&
                b.SessionId == hallSession.SessionId && b.CheckIn == true && b.Status == true).ToListAsync();
                result.Add(new CheckinSession
                {
                    HallName = hallSession.Hall.HallName,
                    SessionNum = (int)hallSession.Session.Session1,
                    BachelorsCheckined = bachelorCheckined.Count,
                    BachelorsSession = bachelorSession.Count
                });

            }
            return result;
        }

        //create new checkin
        public async Task<CheckIn?> CreateCheckinAsync(int hallId, int sessionId)
        {
            var checkInExsit = _context.CheckIns.Any(c => c.HallId == hallId && c.SessionId == sessionId);
            if (checkInExsit)
            {
                return null;
            }
            var checkin = new CheckIn
            {
                HallId = hallId,
                SessionId = sessionId,
                Status = null
            };
            await _context.CheckIns.AddAsync(checkin);
            await _context.SaveChangesAsync();
            return checkin;
        }
        //get all bachelor have table checkin is false and checkIn is false
        public async Task<List<ListBachelor>> GetBachelorCheckInAsync()
        {
            var checkIn = await _context.CheckIns.Where(c => c.Status == false).ToListAsync();
            var result = new List<ListBachelor>();
            foreach (var c in checkIn)
            {
                var hall = await _context.Halls.FirstOrDefaultAsync(h => h.HallId == c.HallId);
                var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionId == c.SessionId);
                var listBachelor = await _context.Bachelors.Where(b => b.HallId == c.HallId && b.SessionId == c.SessionId && b.CheckIn == false).ToListAsync();
                foreach (var b in listBachelor)
                {
                    result.Add(new ListBachelor
                    {
                        Id = b.Id,
                        StudentCode = b.StudentCode,
                        FullName = b.FullName,
                        Mail = b.Mail,
                        Faculty = b.Faculty,
                        Major = b.Major,
                        Image = b.Image,
                        Status = b.Status,
                        StatusBaChelor = b.StatusBaChelor,
                        HallName = hall.HallName,
                        SessionNum = session.Session1,
                        Chair = b.Chair,
                        ChairParent = b.ChairParent,
                        CheckIn = b.CheckIn,
                        TimeCheckIn = b.TimeCheckIn
                    });
                }
            }
            return result;
        }
    }
}
