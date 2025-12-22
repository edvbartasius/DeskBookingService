import { useState } from "react";
import { format, parseISO } from 'date-fns';
import { Button } from "react-bootstrap";

const formatDate = (dateString: string) => {
    try {
        return format(parseISO(dateString), 'MMM dd');
    } catch {
        return dateString;
    }
};

const formatDateRange = (dates: string[]) => {
    if (dates.length === 0) return 'No dates';
    if (dates.length === 1) return formatDate(dates[0]);

    // Just show first and last date as range, or list if few dates
    const sortedDates = [...dates].sort();

    if (sortedDates.length <= 3) {
        return sortedDates.map(formatDate).join(', ');
    }

    // Show first 2 dates and count
    return `${formatDate(sortedDates[0])}, ${formatDate(sortedDates[1])}, +${sortedDates.length - 2} more`;
};

export const DatesDisplay = ({ dates }: { dates: string[] }) => {
    const [showAll, setShowAll] = useState(false);
    const sortedDates = [...dates].sort();
    const displayText = formatDateRange(dates);
    const hasMoreDates = sortedDates.length > 3;

    const fullDates = sortedDates.map(formatDate).join(', ');

    return (
        <div className="d-flex align-items-start w-100">
            <span className="fs-6 fw-bold text-dark flex-grow-1">
                {showAll ? fullDates : displayText}
            </span>

            {hasMoreDates && (
                <Button
                    variant={"outline-secondary"}
                    size="sm"
                    className="ms-2 px-3 text-decoration-none fw-normal text-dark flex-shrink-0"
                    onClick={() => setShowAll(!showAll)}
                >
                    {showAll ? 'Hide' : `Show`}
                </Button>
            )}
        </div>
    );
};