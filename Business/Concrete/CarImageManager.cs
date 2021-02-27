﻿using Business.Abstract;
using Core.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Business.Constant;
using Core.Utilities.Business;
using Core.Utilities.FileOperations;
using Microsoft.AspNetCore.Http;

namespace Business.Concrete
{
    public class CarImageManager : ICarImageService
    {
        ICarImageDal _carImageDal;

        public CarImageManager(ICarImageDal carImageDal)
        {
            _carImageDal = carImageDal;
        }

        public IResult Add(IFormFile imageFile,CarImage carImage)
        {
            carImage.Date=DateTime.Now;
            carImage.ImagePath = FileOperations.Add(imageFile);
            var result = BusinessRules.Run(CheckNumberOfPicture(carImage.CarId),CheckValidFileType(carImage.ImagePath));
            if (result != null)
            {
                return result;
            }
            _carImageDal.Add(carImage);
            return new SuccessResult();
        }

        public IResult Delete(int carImageId)
        {
            CarImage carImage = _carImageDal.Get(ci => ci.CarImageId == carImageId);
            FileOperations.DeleteFile(@carImage.ImagePath);
            _carImageDal.Delete(carImage);
            return new SuccessResult();
        }

        public IDataResult<List<CarImage>> GetAll()
        {
           return new SuccessDataResult<List<CarImage>>(_carImageDal.GetAll());
        }

        public IResult Update(CarImage carImage)
        {
            carImage.Date=DateTime.Now;
            var result = BusinessRules.Run(CheckNumberOfPicture(carImage.CarId));
            if (result != null)
            {
                return result;
            }
            _carImageDal.Update(carImage);
            return new SuccessResult();
        }

        private IResult CheckNumberOfPicture(int carId)
        {
            if (_carImageDal.GetAll(ci => ci.CarId == carId).Count >= 5)
            {
                return new ErrorResult(Messages.NumberOfPictureError);
            }

            return new SuccessResult();
        }

        private IResult CheckValidFileType(string imagePath)
        {
            string [] supportedFileTypes={".jpg",".jpeg","png"};
            int startValue = imagePath.LastIndexOf(".");
            string fileType = imagePath.Substring(startValue);
            for (int i = 0; i < supportedFileTypes.Length; i++)
            {
                if (fileType == supportedFileTypes[i])
                {
                    return new SuccessResult();
                }
            }
            
            return new ErrorResult(Messages.InvalidFileType);

        }
    }
}
