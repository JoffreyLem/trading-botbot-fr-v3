import React, { ReactElement, useState } from "react";

interface TabPanelProps {
  title: string;
  children: React.ReactNode;
}

const TabPanel: React.FC<TabPanelProps> = ({ children }) => {
  return <div>{children}</div>;
};

interface DynamicTabsProps {
  children: ReactElement<TabPanelProps>[] | ReactElement<TabPanelProps>;
}

const DynamicTabs: React.FC<DynamicTabsProps> & {
  TabPanel: React.FC<TabPanelProps>;
} = ({ children }) => {
  const [activeTab, setActiveTab] = useState(0);
  const childArray = React.Children.toArray(
    children,
  ) as ReactElement<TabPanelProps>[];

  const renderActiveTabContent = () => {
    if (childArray[activeTab]) {
      return childArray[activeTab].props.children;
    }
    return null;
  };

  return (
    <div>
      <ul className="flex border-b border-gray-200">
        {childArray.map((child, index) => (
          <li
            key={index}
            className={`cursor-pointer py-2 px-4 ${
              index === activeTab
                ? "border-b-2 border-blue-500 text-blue-500"
                : "text-gray-500 hover:text-blue-500"
            }`}
            onClick={() => setActiveTab(index)}
          >
            {child.props.title}
          </li>
        ))}
      </ul>
      <div className="p-4">{renderActiveTabContent()}</div>
    </div>
  );
};

DynamicTabs.TabPanel = TabPanel;

export default DynamicTabs;
