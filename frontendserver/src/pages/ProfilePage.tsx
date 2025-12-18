import ExpandableCardContainer, { CardConfig } from "../components/ExpandableCardContainer.tsx";

const ProfilePage = () => {
    const cards: CardConfig[] = [
        {
            id: 0,
            title: "User Profile",
            description: "View and edit your profile information",
            allowExpand: false,
            content: undefined
        },
        {
            id: 1,
            title: "Active Reservations",
            description: "View and manage active reservations",
            allowExpand: true,
            content: "Active reservation functionality goes here"
        },
        {
            id: 2,
            title: "Reservation History",
            description: "View and manage reservation history",
            allowExpand: true,
            content: "Reservation history functionality here"
        }
    ];

    return <ExpandableCardContainer cards={cards} title="Profile Page" cardsPerRow={3} />;
}

export default ProfilePage;