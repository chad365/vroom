﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vroom.AppDbContext;
using vroom.Helpers;
using vroom.Models;
using vroom.Models.ViewModels;

namespace vroom.Controllers
{
    public class BikeController : Controller
    {
        private readonly VroomDbContext _db;
        private readonly HostingEnvironment _hostingEnvironment;

        [BindProperty]
        public BikeViewModel BikeVM { get; set; }

        public BikeController(VroomDbContext db, HostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
                
            BikeVM = new BikeViewModel()
            {
                Makes = _db.Makes.ToList(),
                Models = _db.Models.ToList(),
                Bike = new Models.Bike()                         
            };
        }
        public IActionResult Index()
        {
            var Bikes = _db.Bikes.Include(m => m.Make).Include(m =>m.Model);
            return View(Bikes.ToList());
        }

        public IActionResult Create()
        {
            return View(BikeVM);
        }
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePost()
        {
            if (!ModelState.IsValid)
            {
                BikeVM.Makes = _db.Makes.ToList();
                BikeVM.Models = _db.Models.ToList();
                return View(BikeVM);
            }
            _db.Bikes.Add(BikeVM.Bike);
            _db.SaveChanges();

            var BikeID = BikeVM.Bike.Id;

            //Save Image
            string wwrootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var SavedBike = _db.Bikes.Find(BikeID);

            if (files.Count != 0)
            {
                var Extension = Path.GetExtension(files[0].FileName);
                var RelativeImagePath = Image.BikeImagePath + BikeID + Extension;
                var AbsImagePath = Path.Combine(wwrootPath, RelativeImagePath);


                using (var filestream = new FileStream(AbsImagePath, FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                SavedBike.ImagePath = RelativeImagePath;
                _db.SaveChanges();
            }


            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost()
        {
            if (!ModelState.IsValid)
            {
                return View(BikeVM);
            }

            //Save Image
            string wwrootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var BikeID = BikeVM.Bike.Id;
            var SavedBike = _db.Bikes.Find(BikeID);

            if (files.Count != 0)
            {
                var Extension = Path.GetExtension(files[0].FileName);
                var RelativeImagePath = Image.BikeImagePath + BikeID + Extension;
                var AbsImagePath = Path.Combine(wwrootPath, RelativeImagePath);


                using (var filestream = new FileStream(AbsImagePath, FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                SavedBike.ImagePath = RelativeImagePath;
            }
            SavedBike.MakeID = BikeVM.Bike.MakeID;
            SavedBike.ModelID= BikeVM.Bike.ModelID;
            SavedBike.Year = BikeVM.Bike.Year;            
            SavedBike.Mileage = BikeVM.Bike.Mileage;
            SavedBike.Price = BikeVM.Bike.Price;
            SavedBike.Currency = BikeVM.Bike.Currency;
            SavedBike.Features = BikeVM.Bike.Features;
            SavedBike.SellerName = BikeVM.Bike.SellerName;
            SavedBike.SellerEmail = BikeVM.Bike.SellerEmail;
            SavedBike.SellerPhone = BikeVM.Bike.SellerPhone;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        //HTTP Get Method
        [HttpGet]
        public IActionResult Edit(int id)
        {
            BikeVM.Bike = _db.Bikes.Include(m => m.Make).Include(m => m.Model).SingleOrDefault(m => m.Id == id);
            if (BikeVM.Bike == null)
            {
                return NotFound();
            }

            return View(BikeVM);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Bike Bike = _db.Bikes.Find(id);
            if (Bike == null)
            {
                return NotFound();
            }
            _db.Bikes.Remove(Bike);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}