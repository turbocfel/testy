const nav = document.getElementById("site-nav");
const toggle = document.querySelector(".nav-toggle");
const toast = document.getElementById("toast");
const bagCount = document.getElementById("bag-count");

let itemsInBag = 0;

toggle.addEventListener("click", () => {
  const open = nav.classList.toggle("open");
  toggle.setAttribute("aria-expanded", String(open));
});

nav.querySelectorAll("a").forEach((link) => {
  link.addEventListener("click", () => {
    nav.classList.remove("open");
    toggle.setAttribute("aria-expanded", "false");
  });
});

document.querySelectorAll("[data-product]").forEach((button) => {
  button.addEventListener("click", () => {
    itemsInBag += 1;
    bagCount.textContent = `Bag: ${itemsInBag}`;
    const name = button.getAttribute("data-product");
    toast.hidden = false;
    toast.textContent = `${name} added to bag`;
    window.clearTimeout(toast._hide);
    toast._hide = window.setTimeout(() => {
      toast.hidden = true;
    }, 1800);
  });
});
