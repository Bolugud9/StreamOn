// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    // Attach click event to the submit button
    document.getElementById('likeBtn').addEventListener('click', function () {
        // Serialize form data
        //var formData = new FormData(document.getElementById('myForm'));
        var data = {
            movieId: document.getElementById('like-movie-id').value
        }

        var link = `/User/Like?movieId=${data.movieId}`;
        // Make a fetch request to the server
        fetch(link, {
            method: 'GET',  // Change to 'GET' if your server accepts GET requests
        })
            .then(response => response.json())
            .then(data => {
                // Handle the success response
                console.log(data);
            })
            .catch(error => {
                // Handle the error response
                console.error(error);
            });
    });

    document.getElementById('dislikeBtn').addEventListener('click', function () {
        // Serialize form data
        //var formData = new FormData(document.getElementById('myForm'));
        var data = {
            movieId: document.getElementById('like-movie-id').value
        }

        var link = `/User/Dislike?movieId=${data.movieId}`;
        // Make a fetch request to the server
        fetch(link, {
            method: 'GET',  // Change to 'GET' if your server accepts GET requests
        })
            .then(response => response.json())
            .then(data => {
                // Handle the success response
                console.log(data);
            })
            .catch(error => {
                // Handle the error response
                console.error(error);
            });
    });
});