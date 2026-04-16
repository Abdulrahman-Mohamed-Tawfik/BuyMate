(function () {
	'use strict';

	let currentImageIndex = 0;
	let isModalZoomed = false;

	window.setMainImage = function(src, index) {
		const img = document.getElementById('mainImage');
		const modalImg = document.getElementById('modalImage');
		if (img) img.src = src;
		if (modalImg) {
			modalImg.src = src;
			isModalZoomed = false;
			modalImg.style.cursor = 'zoom-in';
			modalImg.style.transformOrigin = 'center center';
			modalImg.style.transform = 'translate(0px, 0px) scale(1)';
		}

		if (index !== undefined) {
			currentImageIndex = index;
		} else if (typeof window.productImages !== 'undefined') {
			currentImageIndex = window.productImages.indexOf(src);
			if(currentImageIndex === -1) currentImageIndex = 0;
		}
	};

	window.prevImage = function(e) {
		if (e) {
			e.preventDefault();
			e.stopPropagation();
		}
		if (typeof window.productImages === 'undefined' || window.productImages.length === 0) return;
		currentImageIndex = (currentImageIndex - 1 + window.productImages.length) % window.productImages.length;
		window.setMainImage(window.productImages[currentImageIndex], currentImageIndex);
	};

	window.nextImage = function(e) {
		if (e) {
			e.preventDefault();
			e.stopPropagation();
		}
		if (typeof window.productImages === 'undefined' || window.productImages.length === 0) return;
		currentImageIndex = (currentImageIndex + 1) % window.productImages.length;
		window.setMainImage(window.productImages[currentImageIndex], currentImageIndex);
	};

	window.incrementQty = function() {
		const qtyInput = document.getElementById('quantity');
		if (!qtyInput) return;
		const currentVal = parseInt(qtyInput.value, 10) || 1;
		const maxVal = parseInt(qtyInput.getAttribute('max'), 10) || 1;
		if (currentVal < maxVal) {
			qtyInput.value = currentVal + 1;
		}
	};

	window.decrementQty = function() {
		const qtyInput = document.getElementById('quantity');
		if (!qtyInput) return;
		const currentVal = parseInt(qtyInput.value, 10) || 1;
		if (currentVal > 1) {
			qtyInput.value = currentVal - 1;
		}
	};

	function initZoom() {
		const container = document.getElementById('mainImageContainer');
		const img = document.getElementById('mainImage');
		const lens = document.getElementById('zoomLens');
		const result = document.getElementById('zoomResult');

		// Lightbox navigation
		const prevBtn = document.getElementById('prevBtn');
		const nextBtn = document.getElementById('nextBtn');
		const modalImg = document.getElementById('modalImage');

		// Main image navigation
		const mainPrevBtn = document.getElementById('mainPrevBtn');
		const mainNextBtn = document.getElementById('mainNextBtn');

		if (typeof window.productImages !== 'undefined' && window.productImages.length > 0) {
			if (prevBtn) prevBtn.addEventListener('click', window.prevImage);
			if (nextBtn) nextBtn.addEventListener('click', window.nextImage);
			if (mainPrevBtn) mainPrevBtn.addEventListener('click', window.prevImage);
			if (mainNextBtn) mainNextBtn.addEventListener('click', window.nextImage);

			// Hide arrows if only 1 image
			if (window.productImages.length <= 1) {
				if (prevBtn) prevBtn.style.display = 'none';
				if (nextBtn) nextBtn.style.display = 'none';
				if (mainPrevBtn) mainPrevBtn.style.display = 'none';
				if (mainNextBtn) mainNextBtn.style.display = 'none';
			}
		}

		if (modalImg) {
			isModalZoomed = false;
			modalImg.style.cursor = 'zoom-in';
			modalImg.style.transition = 'transform 0.2s ease-out';

			let isDragging = false;
			let hasDragged = false;
			let startX, startY;
			let translateX = 0, translateY = 0;

			modalImg.addEventListener('mousedown', function(e) {
				if (isModalZoomed) {
					e.preventDefault();
					isDragging = true;
					hasDragged = false;
					startX = e.clientX - translateX;
					startY = e.clientY - translateY;
					modalImg.style.cursor = 'grabbing';
					modalImg.style.transition = 'none'; // remove transition for smooth drag
				}
			});

			window.addEventListener('mousemove', function(e) {
				if (isDragging && isModalZoomed) {
					let currentTranslateX = e.clientX - startX;
					let currentTranslateY = e.clientY - startY;
					if (Math.abs(currentTranslateX - translateX) > 2 || Math.abs(currentTranslateY - translateY) > 2) {
						hasDragged = true;
					}
					translateX = currentTranslateX;
					translateY = currentTranslateY;
					modalImg.style.transform = `translate(${translateX}px, ${translateY}px) scale(2.5)`;
				}
			});

			window.addEventListener('mouseup', function() {
				if (isDragging) {
					isDragging = false;
					modalImg.style.cursor = 'zoom-out';
					modalImg.style.transition = 'transform 0.2s ease-out';
				}
			});

			modalImg.addEventListener('click', function(e) {
				if (hasDragged && isModalZoomed) {
					hasDragged = false;
					return;
				}

				if (!isModalZoomed) {
					isModalZoomed = true;
					translateX = 0;
					translateY = 0;
					modalImg.style.cursor = 'zoom-out';
					const rect = modalImg.getBoundingClientRect();
					const x = (e.clientX - rect.left) / rect.width * 100;
					const y = (e.clientY - rect.top) / rect.height * 100;
					modalImg.style.transformOrigin = `${x}% ${y}%`;
					modalImg.style.transform = 'scale(2.5)';
				} else {
					isModalZoomed = false;
					modalImg.style.cursor = 'zoom-in';
					modalImg.style.transformOrigin = 'center center';
					modalImg.style.transform = 'translate(0px, 0px) scale(1)';
					translateX = 0;
					translateY = 0;
				}
			});

			// Reset zoom when modal closes
			const modalEl = document.getElementById('imageModal');
			if (modalEl) {
				modalEl.addEventListener('hidden.bs.modal', function () {
					isModalZoomed = false;
					modalImg.style.cursor = 'zoom-in';
					modalImg.style.transformOrigin = 'center center';
					modalImg.style.transform = 'translate(0px, 0px) scale(1)';
					translateX = 0;
					translateY = 0;
				});
			}
		}

		if (container && img && lens && result) {
			// Calculate ratio between result div and lens once image is loaded
			let cx, cy;

			container.addEventListener('mouseenter', function() {
				lens.style.display = "block";
				result.style.display = "block";

				// Setup result background
				result.style.backgroundImage = `url('${img.src}')`;

				// Let's set a generic zoom ratio (e.g., 1.5x)
				const zoomRatio = 1.5;
				result.style.backgroundSize = (img.width * zoomRatio) + "px " + (img.height * zoomRatio) + "px";

				// Set lens size based on result div size and zoom ratio
				lens.style.width = (result.offsetWidth / zoomRatio) + "px";
				lens.style.height = (result.offsetHeight / zoomRatio) + "px";

				cx = result.offsetWidth / lens.offsetWidth;
				cy = result.offsetHeight / lens.offsetHeight;
			});

			container.addEventListener('mousemove', function (e) {
				e.preventDefault();

				const pos = getCursorPos(e, img);
				let x = pos.x - (lens.offsetWidth / 2);
				let y = pos.y - (lens.offsetHeight / 2);

				// Prevent lens from going outside the image
				if (x > img.width - lens.offsetWidth) { x = img.width - lens.offsetWidth; }
				if (x < 0) { x = 0; }
				if (y > img.height - lens.offsetHeight) { y = img.height - lens.offsetHeight; }
				if (y < 0) { y = 0; }

				// Set position of the lens
				lens.style.left = x + "px";
				lens.style.top = y + "px";

				// Set position of the background image in the result div
				result.style.backgroundPosition = "-" + (x * cx) + "px -" + (y * cy) + "px";
			});

			container.addEventListener('mouseleave', function () {
				lens.style.display = "none";
				result.style.display = "none";
			});

			function getCursorPos(e, imgNode) {
				let a, x = 0, y = 0;
				e = e || window.event;
				a = imgNode.getBoundingClientRect();
				x = e.pageX - a.left;
				y = e.pageY - a.top;
				x = x - window.pageXOffset;
				y = y - window.pageYOffset;
				return { x: x, y: y };
			}
		}
	}

	function initReviewStars() {
		const stars = document.querySelectorAll('.star-select');
		const ratingInput = document.getElementById('selectedRating');
		const form = document.getElementById('writeReviewForm');

		if (!stars || stars.length === 0) return;

		function highlightStars(rating) {
			stars.forEach(s => {
				const starRating = parseInt(s.getAttribute('data-rating'), 10);
				if (starRating <= rating) {
					s.classList.remove('far');
					s.classList.add('fas');
				} else {
					s.classList.remove('fas');
					s.classList.add('far');
				}
			});
		}

		stars.forEach(star => {
			star.addEventListener('mouseover', function() {
				const rating = parseInt(this.getAttribute('data-rating'), 10);
				highlightStars(rating);
			});

			star.addEventListener('click', function() {
				const rating = parseInt(this.getAttribute('data-rating'), 10);
				if (ratingInput) ratingInput.value = rating;
				highlightStars(rating);
			});
		});

		const selector = document.getElementById('starRatingSelector');
		if (selector) {
			selector.addEventListener('mouseleave', function() {
				if (ratingInput) {
					highlightStars(parseInt(ratingInput.value, 10) || 0);
				}
			});
		}

		if (form) {
			form.addEventListener('submit', function(e) {
				e.preventDefault();
				if (!ratingInput || ratingInput.value === '0') {
					alert('Please select a rating');
					return;
				}
				alert('Review submitted successfully! (Demo)');
				const modalEl = document.getElementById('reviewModal');
				if (modalEl && window.bootstrap) {
					const m = window.bootstrap.Modal.getInstance(modalEl);
					if (m) m.hide();
				}
				form.reset();
				if (ratingInput) ratingInput.value = '0';
				highlightStars(0);
			});
		}
	}

	function initialize() {
		initZoom();
		initReviewStars();
	}

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', initialize);
	} else {
		initialize();
	}

})();