// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
  // Initialize Notyf
  const notyf = new Notyf({
    duration: 3000,
    position: { x: "right", y: "top" },
    types: [
      {
        type: "success",
        background: "#28a745",
        icon: false,
      },
      {
        type: "error",
        background: "#dc3545",
        icon: false,
      },
    ],
  });

  // Handle like button clicks
  document.querySelectorAll(".like-form").forEach((form) => {
    form.addEventListener("submit", async function (e) {
      e.preventDefault();

      const button = form.querySelector("button");
      const icon = button.querySelector("i");
      const likeCount = form.querySelector(".like-count");
      const formData = new FormData(form);

      // Disable button during request
      button.disabled = true;

      try {
        const response = await fetch(form.action, {
          method: "POST",
          body: formData,
          headers: {
            RequestVerificationToken: form.querySelector(
              'input[name="__RequestVerificationToken"]'
            ).value,
          },
        });

        if (response.ok) {
          const result = await response.json();

          // Update like count
          likeCount.textContent = result.likesCount;

          // Toggle heart icon and add animation
          if (result.isLiked) {
            icon.classList.remove("far");
            icon.classList.add("fas", "heart-beat");
            button.classList.add("liked");
            notyf.success("Вподобано!");
          } else {
            icon.classList.remove("fas", "heart-beat");
            icon.classList.add("far");
            button.classList.remove("liked");
            notyf.success("Вподобання видалено");
          }

          // Remove animation class after animation completes
          setTimeout(() => {
            icon.classList.remove("heart-beat");
          }, 1000);
        } else if (response.status === 401) {
          window.location.href = "/Identity/Account/Login";
        } else {
          notyf.error("Помилка при обробці вподобання");
        }
      } catch (error) {
        console.error("Error:", error);
        notyf.error("Помилка при обробці вподобання");
      } finally {
        // Re-enable button
        button.disabled = false;
      }
    });
  });

  document.querySelectorAll(".like-button").forEach((button) => {
    button.addEventListener("click", function (e) {
      e.preventDefault();
      const form = this.closest("form");
      const likesCount = this.querySelector(".likes-count");
      const icon = this.querySelector("i");

      fetch(form.action, {
        method: "POST",
        headers: {
          "Content-Type": "application/x-www-form-urlencoded",
          RequestVerificationToken: form.querySelector(
            'input[name="__RequestVerificationToken"]'
          ).value,
        },
        body: new URLSearchParams(new FormData(form)),
      })
        .then((response) => response.json())
        .then((data) => {
          if (data.success) {
            likesCount.textContent = data.likesCount;
            icon.classList.toggle("fas", data.isLiked);
            icon.classList.toggle("far", !data.isLiked);
          } else {
            console.error("Ошибка:", data.message);
          }
        })
        .catch((error) => console.error("Ошибка:", error));
    });
  });
});

// Add CSS for heart animation
const style = document.createElement("style");
style.textContent = `
    .heart-beat {
        animation: heartBeat 1s ease-in-out;
    }
    
    @keyframes heartBeat {
        0% { transform: scale(1); }
        14% { transform: scale(1.3); }
        28% { transform: scale(1); }
        42% { transform: scale(1.3); }
        70% { transform: scale(1); }
    }
    
    .like-button {
        transition: all 0.3s ease;
        border: none;
        background: none;
        padding: 0.5rem;
        cursor: pointer;
    }
    
    .like-button:hover {
        transform: scale(1.1);
    }
    
    .like-button.liked {
        color: #e31b23;
    }
    
    .like-button:disabled {
        opacity: 0.7;
        cursor: not-allowed;
    }
`;
document.head.appendChild(style);
