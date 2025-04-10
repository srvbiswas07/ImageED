﻿using ImageED.Helpers;
using ImageED.Models;
using static Java.Security.KeyStore;

namespace ImageED
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseHelper _dbHelper;
        private byte[] _currentImageData;
        private ImageEntry _selectedImage;

        public MainPage()
        {
            InitializeComponent();
            _dbHelper = new DatabaseHelper();
            LoadImagesList();
            PreviewImage.Source = "upload.png"; 
        }

        private async void LoadImagesList()
        {
            try
            {
                var images = await _dbHelper.GetAllImagesAsync();
                ImageList.ItemsSource = images.OrderByDescending(i => i.CreatedAt);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load images: " + ex.Message, "OK");
            }
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images
                });

                if (result != null)
                {
                    using var stream = await result.OpenReadAsync();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    _currentImageData = memoryStream.ToArray();

                    PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(_currentImageData));

                    ImageList.SelectedItem = null;
                    _selectedImage = null;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load image: " + ex.Message, "OK");
            }
        }

        private async void OnEncryptClicked(object sender, EventArgs e)
        {
            if (_currentImageData == null)
            {
                await DisplayAlert("Error", "Please select an image first", "OK");
                return;
            }

            try
            {
                string secretKey = SecretKeyEntry.Text;
                var (encryptedData, iv, salt) = EncryptionHelper.EncryptImage(_currentImageData, secretKey);

                var entry = new ImageEntry
                {
                    Name = $"Image_{DateTime.Now:yyyyMMddHHmmss}",
                    EncryptedData = encryptedData,
                    IV = iv,
                    Salt = salt,
                    CreatedAt = DateTime.Now
                };

                await _dbHelper.SaveImageAsync(entry);
                await DisplayAlert("Success", "Image encrypted and saved successfully", "OK");

                LoadImagesList();

                _currentImageData = null;
                PreviewImage.Source = "upload.png";
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to encrypt image: " + ex.Message, "OK");
            }
        }

        private void OnImageSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ImageEntry selected)
            {
                _selectedImage = selected;
                _currentImageData = null;
                PreviewImage.Source = "upload.png"; 
            }
        }

        private async void OnDecryptClicked(object sender, EventArgs e)
        {
            if (_selectedImage == null)
            {
                await DisplayAlert("Error", "Please select an image from the list to decrypt", "OK");
                return;
            }

            try
            {
                string secretKey = SecretKeyEntry.Text;
                var decryptedData = EncryptionHelper.DecryptImage(
                    _selectedImage.EncryptedData,
                    _selectedImage.IV,
                    _selectedImage.Salt,
                    secretKey
                );

                PreviewImage.Source = ImageSource.FromStream(() => new MemoryStream(decryptedData));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to decrypt image: " + ex.Message, "OK");
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int imageId)
            {
                await DeleteImage(imageId);
            }
        }

        private async void OnDeleteSwipe(object sender, EventArgs e)
        {
            if (sender is SwipeItem swipeItem && swipeItem.BindingContext is ImageEntry entry)
            {
                await DeleteImage(entry.Id);
            }
        }

        private async Task DeleteImage(int imageId)
        {
            bool answer = await DisplayAlert("Confirm Delete",
                "Are you sure you want to delete this image?",
                "Yes", "No");

            if (answer)
            {
                try
                {
                    await _dbHelper.DeleteImageAsync(imageId);

                    if (_selectedImage?.Id == imageId)
                    {
                        _selectedImage = null;
                        PreviewImage.Source = null;
                        PreviewImage.Source = "upload.png"; 
                    }

                    LoadImagesList();

                    await DisplayAlert("Success", "Image deleted successfully", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error",
                        "Failed to delete image: " + ex.Message,
                        "OK");
                }
            }
        }


        private void OnTogglePasswordVisibility(object sender, EventArgs e)
        {
            SecretKeyEntry.IsPassword = !SecretKeyEntry.IsPassword;
            TogglePasswordButton.Text = SecretKeyEntry.IsPassword ? "👁️" : "👁️‍🗨️";
        }
    }

}
