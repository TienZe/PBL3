﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PBL3.Infrastructures;
using PBL3.Models.Domain;
using PBL3.Models.DTO;
using PBL3.Repositories.Abstract;
using PBL3.Repositories.Implementation;
using System.Xml.Linq;

namespace PBL3.Controllers
{
    [Authorize(Roles = Role.Admin)]
    public class SongController : Controller
    {
        private readonly ISongRepository songRepository;
        private readonly IRepository<Genre> genreRepository;
        private readonly IEmotionRepository emotionRepository;
        private readonly FileService fileService;
        private const int PageSize = 10;
        public SongController(ISongRepository songService, IRepository<Genre> genreService
            , IEmotionRepository emotionService, FileService fileService) 
        {
            this.songRepository = songService;
            this.genreRepository = genreService;
            this.emotionRepository = emotionService;
            this.fileService = fileService;
        }
        public IActionResult Index(int? pageIndex, string searchString)
        {
            // Server validation
            pageIndex = (pageIndex == null || pageIndex < 1) ? 1 : pageIndex;

            var listSongs = songRepository.GetAllWithRelatedGenreAndEmotion();

            // Tiếp tục mở rộng truy vấn
            if (!String.IsNullOrEmpty(searchString))
            {
                // Thực hiện Search
                listSongs = listSongs.Where(s => s.Name.Contains(searchString));
            }
            // Truyền filter hiện tại sang cho View
            ViewData["SearchString"] = searchString ?? string.Empty;

            // Sắp xếp
            listSongs = listSongs.OrderBy(s => s.SongId);

            // Phân trang kết quả
            return View(PaginatedList<Song>.CreateAsync(listSongs, pageIndex.Value, PageSize));
        }
        private void AddSelectListToView()
        {
            var emotions = emotionRepository.GetAll().OrderBy(e => e.Name).Select(e => new SelectListItem()
                { Text = e.Name, Value = e.EmotionId.ToString() });
            var genres = genreRepository.GetAll().OrderBy(g => g.Name).Select(g => new SelectListItem()
                { Text = g.Name, Value = g.GenreId.ToString() });
            ViewData["Emotions"] = emotions;
            ViewData["Genres"] = genres;
        }
        private IActionResult ViewWithSelectList(object? model = null)
        {
            AddSelectListToView();
			return View(model);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return ViewWithSelectList();
		}
        [HttpPost]
        public IActionResult Create(CreateSongModel model)
        {
			if (ModelState.IsValid)
            {
				// Validate hợp lệ, thực hiện thêm Song
				Song song = new Song()
                {
                    Name = model.Name!,
                    Artist = model.Artist!
                };
                
                try
				{
                    // Upload audio và set file name vào song.Source
					song.Source = fileService.UploadAudio(model.Audio!, "audio/song");
				}
                catch (Exception ex) 
                {
                    ModelState.AddModelError(nameof(model.Audio), ex.Message);
					return ViewWithSelectList(model);
				}

                // Set relationship
				songRepository.SetRelatedEmotion(song, model.EmotionIds!);
				songRepository.SetRelatedGenre(song, model.GenreIds!);
                // Add
				songRepository.Add(song);

                return RedirectToAction(nameof(Index));
            }
			return ViewWithSelectList(model);
		}
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Song? song = songRepository.GetByIdWithRelatedEntity(id);
            if (song == null) return NotFound();

            EditSongModel viewModel = new EditSongModel()
            {
                SongId = song.SongId,
                Name = song.Name,
                Artist = song.Artist,
                CurrentAudio = song.Source,
            };
            viewModel.EmotionIds = song.Emotions.Select(e => e.EmotionId);
			viewModel.GenreIds = song.Genres.Select(g => g.GenreId);

			return ViewWithSelectList(viewModel);
		}
        [HttpPost]
        public IActionResult Edit(EditSongModel model)
        {
            if (ModelState.IsValid)
            {
                Song? song = songRepository.GetByIdWithRelatedEntity(model.SongId);
                if (song == null) return NotFound();

                // set
                song.Name = model.Name!;
                song.Artist = model.Artist!;
                // Có thể upload audio mới và xóa audio hiện tại
                if (model.NewAudio != null)
                {
                    string oldAudio = song.Source;
					try
					{
                        // Upload new audio và set file name cho Source
						song.Source = fileService.UploadAudio(model.NewAudio, "audio/song");
					}
					catch (Exception ex)
					{
						ModelState.AddModelError(nameof(model.NewAudio), ex.Message);
						return ViewWithSelectList();
					}
                    // Delete current audio
                    if (!String.IsNullOrEmpty(oldAudio))
                    {
						fileService.DeleteFile("audio/song/" + oldAudio);
					}
				}
                // set relationship
                songRepository.SetRelatedEmotion(song, model.EmotionIds!);
                songRepository.SetRelatedGenre(song, model.GenreIds!);
                // update
                songRepository.Update(song);

				return RedirectToAction(nameof(Index));
			}
			return ViewWithSelectList(model);
		}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            // Xóa audio tương ứng
            Song? song = songRepository.GetById(id);
            if (song != null && !String.IsNullOrEmpty(song.Source))
            {
                fileService.DeleteFile("audio/song/" + song.Source);
            }
            songRepository.Delete(id);

            return RedirectToAction(nameof(Index));
        }
    }
}

