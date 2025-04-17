document.addEventListener("DOMContentLoaded", function () {
  // Анимация карточек при скролле
  const cards = document.querySelectorAll(".documentation-card");

  function checkScroll() {
    const triggerBottom = (window.innerHeight / 5) * 4;

    cards.forEach((card) => {
      const cardTop = card.getBoundingClientRect().top;

      if (cardTop < triggerBottom) {
        card.classList.add("show");
      }
    });
  }

  // Инициализация карточек
  cards.forEach((card) => {
    card.classList.add("fade-up");
  });

  // Проверяем позицию при загрузке и скролле
  checkScroll();
  window.addEventListener("scroll", checkScroll);

  // Плавная прокрутка для навигации
  const navLinks = document.querySelectorAll(".documentation-nav a");

  navLinks.forEach((link) => {
    link.addEventListener("click", function (e) {
      e.preventDefault();

      const targetId = this.getAttribute("href").substring(1);
      const targetElement = document.getElementById(targetId);

      if (targetElement) {
        window.scrollTo({
          top: targetElement.offsetTop - 100,
          behavior: "smooth",
        });
      }
    });
  });

  // Подсветка активного раздела в навигации
  function updateActiveSection() {
    const sections = document.querySelectorAll("section[id]");

    sections.forEach((section) => {
      const sectionTop = section.offsetTop - 150;
      const sectionHeight = section.offsetHeight;
      const scroll = window.scrollY;

      if (scroll > sectionTop && scroll < sectionTop + sectionHeight) {
        const correspondingLink = document.querySelector(
          `.documentation-nav a[href="#${section.id}"]`
        );
        if (correspondingLink) {
          document.querySelectorAll(".documentation-nav a").forEach((link) => {
            link.classList.remove("active");
          });
          correspondingLink.classList.add("active");
        }
      }
    });
  }

  window.addEventListener("scroll", updateActiveSection);

  // Анимация кнопок скачивания
  const downloadButtons = document.querySelectorAll(".btn-download");

  downloadButtons.forEach((button) => {
    button.addEventListener("click", function () {
      this.classList.add("pulse");
      setTimeout(() => {
        this.classList.remove("pulse");
      }, 1000);
    });
  });
});
