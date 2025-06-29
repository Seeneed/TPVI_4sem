//document.addEventListener('DOMContentLoaded', () => {

//    const celebritiesContainer = document.getElementById('celebrities-container');
//    const lifeEventsContainer = document.getElementById('life-events-container');

//    if (!celebritiesContainer || !lifeEventsContainer) {
//        console.error('HTML Error: Required container(s) not found (#celebrities-container or #life-events-container).');
//        if (document.body) {
//            document.body.innerHTML = '<h1>Error</h1><p>Page structure is broken. Please contact support.</p>';
//        }
//        return;
//    }

//    async function filterAndDisplayLifeEvents(clickedCelebrityId, celebrityName) {
//        lifeEventsContainer.innerHTML = '<p>Loading events...</p>';

//        const apiUrl = '/api/Lifeevents';

//        try {
//            const response = await fetch(apiUrl);

//            if (!response.ok) {
//                throw new Error(`Network error fetching all events: ${response.status} ${response.statusText}`);
//            }

//            const allLifeEvents = await response.json();
//            lifeEventsContainer.innerHTML = '';

//            if (!Array.isArray(allLifeEvents)) {
//                throw new Error('Invalid data format received for all life events (expected an array).');
//            }

//            const clickedIdAsNumber = parseInt(clickedCelebrityId, 10);
//            const filteredEvents = allLifeEvents.filter(event => event.celebrityId === clickedIdAsNumber);

//            if (filteredEvents.length === 0) {
//                lifeEventsContainer.innerHTML = `<p>No life events found for ${celebrityName}.</p>`;
//                return;
//            }

//            const list = document.createElement('ul');
//            filteredEvents.forEach(lifeEvent => {
//                const li = document.createElement('li');
//                const formattedDate = lifeEvent.date ? lifeEvent.date.split('T')[0] : 'No date';
//                li.textContent = `${celebrityName} ${formattedDate} ${lifeEvent.description || ''}`;
//                list.appendChild(li);
//            });
//            lifeEventsContainer.appendChild(list);

//        } catch (error) {
//            console.error('Failed to load or display life events:', error);
//            lifeEventsContainer.innerHTML = `<p>Error loading life events for ${celebrityName}. Please try again later.</p>`;
//        }
//    }

//    function handleImageClick(event) {
//        const clickedImage = event.target;
//        const celebrityId = clickedImage.dataset.celebrityId;
//        const celebrityName = clickedImage.dataset.celebrityName;

//        if (celebrityId && celebrityName) {
//            document.querySelectorAll('#celebrities-container img.selected').forEach(img => {
//                img.classList.remove('selected');
//            });
//            clickedImage.classList.add('selected');
//            filterAndDisplayLifeEvents(celebrityId, celebrityName);
//        } else {
//            console.error('Click handler error: Could not get celebrity ID or Name from image data attribute.');
//        }
//    }

//    async function loadCelebrities() {
//        try {
//            const response = await fetch('/api/Celebrities');
//            if (!response.ok) {
//                throw new Error(`Network error fetching celebrities: ${response.status} ${response.statusText}`);
//            }

//            const celebrities = await response.json();

//            if (!Array.isArray(celebrities)) {
//                throw new Error('Invalid data format received for celebrities (expected an array).');
//            }

//            celebritiesContainer.innerHTML = '';

//            if (celebrities.length === 0) {
//                celebritiesContainer.innerHTML = '<p>No celebrities found.</p>';
//                return;
//            }

//            celebrities.forEach(celebrity => {
//                if (celebrity.reqPhotoPath && celebrity.id) {
//                    const img = document.createElement('img');
//                    img.src = `/api/Celebrities/photo/${encodeURIComponent(celebrity.reqPhotoPath)}`;
//                    img.alt = celebrity.fullName || 'Celebrity Photo';
//                    img.dataset.celebrityId = celebrity.id;
//                    img.dataset.celebrityName = celebrity.fullName;
//                    img.addEventListener('click', handleImageClick);
//                    celebritiesContainer.appendChild(img);
//                } else {
//                    console.warn(`Skipping celebrity: ${celebrity.fullName || 'Unknown'} (ID: ${celebrity.id || 'N/A'}) due to missing photo path or ID.`);
//                }
//            });

//        } catch (error) {
//            console.error('Failed to load celebrities:', error);
//            celebritiesContainer.innerHTML = `<p>Error loading celebrities. Please check the connection or contact support. Details: ${error.message}</p>`;
//            lifeEventsContainer.innerHTML = '';
//        }
//    }

//    loadCelebrities();

//});